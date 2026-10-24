"""Exercise Web Forms rendering and postbacks in the disposable IIS test host."""
import http.cookiejar
import re
import sys
import time
import urllib.parse
import urllib.request
from html.parser import HTMLParser

BASE = sys.argv[1]
PAGE = BASE + "/Pages/Chef/CreateCourse/CreateCoursePage.aspx"

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
            link = next(link for link in form.links if link.get('id', '').endswith(target))
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
assert 'Basic information' in browser.html
browser.post('Save & next')
assert 'Please enter the course title.' in browser.html
browser.post('Save & next', {'txtCourseTitle': 'Integration course', 'txtDescription': 'Learn a dish.', 'ddlCategory': '1', 'ddlDifficulty': 'Beginner'})
assert 'recipe-form' in browser.html
course_id = re.search(r'data-course-id="(\d+)"', browser.html).group(1)
assert int(course_id) > 0
browser.post('Save & next', {'txtRecipeName': 'Test dish', 'txtIngredients': 'Fish\nSalt', 'txtCookingTime': '30', 'txtServingSize': '2'})
assert 'No lessons added yet.' in browser.html
browser.post('+ Add first lesson')
for index in range(1, 7):
    form = Form(browser.html)
    titles = [name for name in form.values if name.endswith('txtLessonTitle')]
    instructions = [name for name in form.values if name.endswith('txtLessonInstructions')]
    edits = {titles[-1]: 'Cooking step ' + str(index), instructions[-1]: 'Instruction ' + str(index)}
    browser.post('+ Add another lesson' if index < 6 else 'Save draft', edits)
assert 'Page 2 of 2 (6 lessons)' in browser.html
browser.post('Previous')
assert 'Cooking step 1' in browser.html and 'Cooking step 5' in browser.html
assert 'Cooking step 6' not in browser.html
browser.post('Move down', button_index=0)
browser.post('Move up', button_index=1)
browser.post('Next')
assert 'Cooking step 6' in browser.html
browser.post('Delete lesson')
assert 'Page 2 of 2' not in browser.html
browser.post('Save & next')
assert 'quiz-form' in browser.html
browser.post('+ Add first question', {'txtQuizTitle': 'Knowledge check'})
for index in range(1, 7):
    form = Form(browser.html)
    question = [name for name in form.values if name.endswith('txtQuestionText')][-1]
    options = [name for name in form.values if name.endswith('txtOptionText')][-2:]
    radio = [item for item in form.inputs if item.get('type') == 'radio'][-2]
    edits = {question: 'Question ' + str(index), options[0]: 'Correct ' + str(index), options[1]: 'Other ' + str(index)}
    browser.post('+ Add another question' if index < 6 else 'Save draft', edits, radio=radio)
assert 'Page 2 of 2' in browser.html
browser.post('Previous')
assert 'Question 1' in browser.html and 'Question 5' in browser.html
form = Form(browser.html)
assert len([item for item in form.inputs if item.get('type') == 'radio' and 'checked' in item]) == 5
browser.post('+ Add option', button_index=0)
browser.post('Save & next')
assert 'Fill in every option for Question 1.' in browser.html
form = Form(browser.html)
assert len([item for item in form.inputs if item.get('type') == 'radio' and 'checked' in item]) == 5
browser.post('Remove', button_index=2)
browser.post('Save & next')
assert 'activity-form' in browser.html
browser.post('Save & next', {'txtActivityTitle': 'Try it yourself', 'txtActivityInstructions': 'Cook and share a photo.'})
assert 'course-preview' in browser.html and 'Integration course' in browser.html
assert '6 questions' in browser.html and 'Correct answer' in browser.html
browser.get(PAGE + '?courseId=' + course_id)
assert 'Integration course' in browser.html
browser.post(target='btnStep2')
assert 'Cooking step 1' in browser.html
browser.post(target='btnStep3')
form = Form(browser.html)
assert len([item for item in form.inputs if item.get('type') == 'radio' and 'checked' in item]) == 5
other = Browser()
other.get(BASE + '/Fixture.aspx?other=1')
other.get(PAGE + '?courseId=' + course_id)
assert 'This draft is unavailable' in other.html
browser.post(target='btnStep5')
browser.post('Submit for review')
assert 'Course submitted for review.' in browser.html
browser.get(PAGE + '?courseId=' + course_id)
assert 'already been submitted for review' in browser.html
print('PASS: draft/save/resume, lessons and quiz paging/editing/order/delete, radio persistence, validation, ownership, preview and Pending submission')
