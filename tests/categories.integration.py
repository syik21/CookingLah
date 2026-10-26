"""Exercise Web Forms rendering and postbacks in the disposable IIS test host."""
import http.cookiejar
import re
import sys
import time
import urllib.parse
import urllib.request
from html.parser import HTMLParser

BASE = sys.argv[1]
PAGE = BASE + "/Pages/Admin/ManageCategories/ManagerCategoriesPage.aspx"

class Form(HTMLParser):
    def __init__(self, html):
        super().__init__()
        self.values, self.buttons, self.inputs, self.links = {}, [], [], []
        self.textarea = self.select = None
        self.feed(html)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'input':
            self.inputs.append(attrs)
            name, kind = attrs.get('name'), attrs.get('type', 'text')
            if kind == 'submit': self.buttons.append(attrs)
            elif name and kind != 'file' and (kind not in ('radio', 'checkbox') or 'checked' in attrs):
                self.values[name] = attrs.get('value', '')
        elif tag == 'textarea':
            self.textarea = attrs.get('name')
            self.values[self.textarea] = ''
        elif tag == 'select':
            self.select = attrs.get('name')
            self.values[self.select] = ''
        elif tag == 'option' and self.select and 'selected' in attrs:
            self.values[self.select] = attrs.get('value', '')
        elif tag == 'a': self.links.append(attrs)

    def handle_endtag(self, tag):
        if tag == 'textarea': self.textarea = None
        if tag == 'select': self.select = None

    def handle_data(self, text):
        if self.textarea: self.values[self.textarea] += text

    def name(self, suffix):
        matches = [name for name in self.values if name.endswith(suffix)]
        assert len(matches) == 1, (suffix, matches)
        return matches[0]

class Browser:
    def __init__(self):
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
        self.html = ''

    def request(self, url, values=None):
        try:
            payload = urllib.parse.urlencode(values).encode() if values is not None else None
            self.html = self.opener.open(url, payload).read().decode('utf-8')
        except urllib.error.HTTPError as error:
            details = error.read().decode('utf-8', errors='replace')
            print(re.sub('<[^>]+>', ' ', details)[:10000])
            raise
        assert 'Server Error in' not in self.html
        return self.html

    def get(self, url): return self.request(url)

    def post(self, button_text=None, edits=None, button_index=0, target=None, radio=None):
        form = Form(self.html)
        values = form.values.copy()
        for suffix, value in (edits or {}).items(): values[form.name(suffix)] = value
        if radio: values[radio['name']] = radio['value']
        if target:
            link = next(link for link in form.links if link.get('aria-label') == target)
            values['__EVENTTARGET'] = re.search(r"__doPostBack\('([^']+)'", link['href']).group(1)
            values['__EVENTARGUMENT'] = ''
        else:
            button = [b for b in form.buttons if b.get('value') == button_text][button_index]
            assert 'disabled' not in button, button
            values[button['name']] = button.get('value', '')
        return self.request(PAGE, values)


browser = Browser()
for attempt in range(30):
    try:
        browser.get(BASE + '/Fixture.aspx')
        break
    except Exception:
        if attempt == 29: raise
        time.sleep(0.5)

browser.get(PAGE)
assert 'Category Management' in browser.html and 'Bread and pastry' in browser.html
assert '1 course' in browser.html
browser.post(target='Delete Baking')
assert 'Cannot delete this category.' in browser.html
assert not any(b.get('value') == 'Delete Category' for b in Form(browser.html).buttons)
browser.post('Cancel')
browser.post(target='Delete Grilling')
assert 'Are you sure you want to delete' in browser.html
# Add a course in a separate request after confirmation was rendered.
browser.opener.open(BASE + '/Fixture.aspx?link=1').read()
browser.post('Delete Category')
assert 'This category cannot be deleted because 1 course is currently using it.' in browser.html
browser.post('Cancel')
browser.post('+ Add Category')
browser.post('Add Category', {'txtCategoryName': '   '})
assert 'Please enter a category name.' in browser.html
browser.post('Add Category', {'txtCategoryName': 'Baking'})
assert 'A category with this name already exists.' in browser.html
browser.post('Add Category', {'txtCategoryName': 'Desserts', 'txtCategoryDescription': 'a' * 501})
assert 'Descriptions must be 500 characters or fewer.' in browser.html
browser.post('Add Category', {'txtCategoryName': 'Desserts', 'txtCategoryDescription': 'Sweet dishes'})
assert 'Category added successfully.' in browser.html
browser.get(PAGE)
assert browser.html.count('aria-label="Edit Desserts"') == 1
browser.post(target='Edit Desserts')
assert 'Sweet dishes' in browser.html
browser.post('Save Changes', {'txtCategoryName': 'Baking'})
assert 'A category with this name already exists.' in browser.html
browser.post('Save Changes', {'txtCategoryName': 'Sweet treats', 'txtCategoryDescription': 'Chocolate and fruit'})
assert 'Category updated successfully.' in browser.html
browser.post('Search', {'txtSearchCategory': 'Chocolate'})
assert 'Sweet treats' in browser.html and 'aria-label="Edit Baking"' not in browser.html
browser.post('Search', {'txtSearchCategory': 'no-match'})
assert 'No categories match your search.' in browser.html
assert '<table>' not in browser.html
browser.post('Clear Search')
browser.post(target='Delete Sweet treats')
assert 'Are you sure you want to delete' in browser.html
browser.post('Cancel')
assert 'Sweet treats' in browser.html
browser.post(target='Delete Sweet treats')
browser.post('Delete Category')
assert 'Category deleted successfully.' in browser.html
assert 'aria-label="Edit Sweet treats"' not in browser.html
for number in range(10):
    browser.post('+ Add Category')
    browser.post('Add Category', {'txtCategoryName': 'Test category %02d' % number})
assert 'Showing 1\u201310 of 12 categories' in browser.html
browser.post('Next')
assert 'Showing 11\u201312 of 12 categories' in browser.html
assert len([a for a in Form(browser.html).links if a.get('aria-label', '').startswith('Edit ')]) == 2
browser.post('Previous')
assert 'Showing 1\u201310 of 12 categories' in browser.html
browser.get(BASE + '/Fixture.aspx?other=1')
try:
    browser.opener.open(PAGE)
    raise AssertionError('Instructor was allowed to manage categories')
except urllib.error.HTTPError as error:
    assert error.code == 403, error.code
print('PASS: rendering, required/length/duplicate validation, add/edit/delete, refresh, search, empty results, pagination, course usage and admin authorization.')
