"""Exercise Web Forms rendering and postbacks in the disposable IIS test host."""
import http.cookiejar
import re
import sys
import time
import urllib.parse
import urllib.request
from html.parser import HTMLParser

BASE = sys.argv[1]
PAGE = BASE + "/Pages/Chef/ManageCourse/ManageCoursePage.aspx"

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
            response = self.opener.open(url, payload)
            self.url = response.url
            self.html = response.read().decode('utf-8')
        except urllib.error.HTTPError as error:
            details = error.read().decode('utf-8', errors='replace')
            print(re.sub('<[^>]+>', ' ', details)[:10000])
            raise
        assert 'Server Error in' not in self.html
        return self.html

    def get(self, url): return self.request(url)

    def post(self, button_text=None, edits=None, button_index=0, target=None, radio=None, target_index=0):
        form = Form(self.html)
        values = form.values.copy()
        for suffix, value in (edits or {}).items(): values[form.name(suffix)] = value
        if radio: values[radio['name']] = radio['value']
        if target:
            link = [link for link in form.links if target in link.get('href', '')][target_index]
            values['__EVENTTARGET'] = re.search(r"__doPostBack\('([^']+)'", link['href']).group(1)
            values['__EVENTARGUMENT'] = ''
        else:
            button = [b for b in form.buttons if b.get('value') == button_text][button_index]
            assert 'disabled' not in button, button
            values[button['name']] = button.get('value', '')
        return self.request(self.url, values)


import json
browser = Browser()
for attempt in range(30):
    try:
        browser.get(BASE + '/Fixture.aspx')
        break
    except Exception:
        if attempt == 29: raise
        time.sleep(0.5)

def visit(tab='overview', course=1):
    return browser.get(PAGE + '?courseId=%s&tab=%s' % (course,tab))

def inspect():
    return json.loads(browser.opener.open(BASE + '/Fixture.aspx?inspect=1').read())

def fixture(query):
    browser.opener.open(BASE + '/Fixture.aspx?' + query).read()

def assert_denied(url):
    try:
        browser.opener.open(url)
        raise AssertionError('Access was unexpectedly allowed: ' + url)
    except urllib.error.HTTPError as error:
        assert error.code in (403,404), error.code

browser.get(PAGE)
assert 'Owned Course' in browser.html and 'Empty Draft' in browser.html
assert 'Private Other Course' not in browser.html

assert 'No course image yet' in browser.html
browser.post('Search', {'txtCourseSearch': 'Owned'})
assert 'Owned Course' in browser.html and 'Empty Draft' not in browser.html
browser.post('Search', {'txtCourseSearch': 'Private Other'})
assert 'No matching courses' in browser.html
browser.post('Search', {'txtCourseSearch': '%'})
assert 'No matching courses' in browser.html
browser.post('Clear')
assert 'Owned Course' in browser.html and 'Empty Draft' in browser.html

browser.post('Draft (1)')
assert 'Empty Draft' in browser.html and 'Owned Course' not in browser.html
browser.post('Published (1)')
assert 'Owned Course' in browser.html and 'Empty Draft' not in browser.html
browser.post('Pending review (0)')
assert 'No matching courses' in browser.html
browser.post('All courses (2)')
assert 'Owned Course' in browser.html and 'Empty Draft' in browser.html


assert_denied(PAGE+'?courseId=2')
assert_denied(PAGE+'?courseId=bad')
for tab in ['overview','recipe','lessons','quiz','activity','preview']:
    visit(tab)
    assert 'Owned Course' in browser.html
visit('preview')
assert 'Correct answer' not in browser.html
visit()
browser.get(PAGE+'?courseId=1&tab=overview&edit=1')
browser.post('Save Changes', {'txtTitle':'Updated Course', 'txtDescription':'Updated description'})
assert 'Course updated successfully.' in browser.html
visit('recipe')
browser.post('Edit Recipe')
browser.post('Save Changes', {'txtIngredients':'[{"name":"Fish","amount":"1 whole"}]','txtCookingTime':'0'})
assert 'Cooking time must be a positive whole number.' in browser.html
browser.post('Save Changes', {'txtCookingTime':'45','txtServingSize':'4'})
assert 'Recipe saved successfully.' in browser.html
assert inspect()['RecipeID']==1
visit('activity')
browser.post('Edit Practical Activity')
browser.post('Save Changes', {'txtTitle':'Updated Activity','txtInstructions':'Cook and upload'})
assert 'Practical Activity saved successfully.' in browser.html
assert inspect()['ActivitySubmissions']==1 and inspect()['ActivityID']==1
visit('lessons')
assert 'Showing Lessons 1-5 of 7' in browser.html
browser.post('Next')
assert 'Showing Lessons 6-7 of 7' in browser.html
browser.post(target="rptLessons$ctl00",target_index=1)
order=inspect()['LessonOrder']
assert order[4]=='6:5' and order[5]=='5:6',order
browser.post(target="rptLessons$ctl00",target_index=0)
browser.post('Save Lesson',{'txtTitle':'Changed Lesson','txtInstructions':'New steps'})
assert 'Lesson updated successfully.' in browser.html
visit('lessons')
browser.post(target="rptLessons$ctl00",target_index=2)
assert 'Delete Lesson?' in browser.html
browser.post('Delete Lesson Permanently')
order=inspect()['LessonOrder']
assert len(order)==6 and [int(x.split(':')[1]) for x in order]==list(range(1,7)),order
visit('quiz')
assert 'Showing Questions 1-5 of 7' in browser.html
browser.post('Next')
assert 'Showing Questions 6-7 of 7' in browser.html
browser.post('Edit Quiz')
browser.post('Save Quiz',{'txtQuizTitle':'Updated Quiz','txtPassingScore':'70'})
assert inspect()['QuizAttempts']==1 and inspect()['QuizID']==1
visit('quiz')
browser.post(target="rptQuestions$ctl00",target_index=0)
browser.post(target="rptEditOptions$ctl01",target_index=0)
browser.post('Save Question')
assert 'A question must contain at least two answer options.' in browser.html
browser.post('+ Add Option')
form=Form(browser.html)
option_names=[name for name in form.values if name.endswith('txtOptionText')]
radio=[r for r in form.inputs if r.get('type')=='radio'][-1]
browser.post('Save Question',{option_names[0]:'First answer',option_names[1]:'Second answer','txtQuestionText':'Updated question?'},radio=radio)
assert 'Question saved successfully.' in browser.html
assert inspect()['QuizAttempts']==1
visit('quiz')
browser.post(target="rptQuestions$ctl00",target_index=1)
browser.post('Delete Permanently')
assert inspect()['QuizQuestions']==6 and inspect()['QuizOptions']==12
visit('recipe')
browser.post('Edit Recipe')
fixture('status=Pending')
browser.post('Save Changes',{'txtIngredients':'[{"name":"Must not save","amount":"1 g"}]'})
assert 'cannot be edited while it is under review' in browser.html
visit('recipe')
assert 'Must not save' not in browser.html
assert 'Editing is temporarily disabled' in browser.html
assert not any(b.get('value')=='Edit Recipe' for b in Form(browser.html).buttons)
fixture('status=Rejected')
visit('recipe')
browser.post('Delete Recipe')
browser.post('Cancel')
assert inspect()['CourseRecipes']==1
browser.post('Delete Recipe')
browser.post('Delete Recipe Permanently')
assert inspect()['CourseRecipes']==0 and inspect()['Courses']==3
browser.post('+ Add Recipe')
browser.post('Save Changes',{'txtIngredients':'[{"name":"Fish","amount":"1 whole"}]'})
assert 'Recipe saved successfully.' in browser.html
visit('activity')
browser.post('Delete Practical Activity')
browser.post('Delete Practical Activity Permanently')
assert inspect()['ActivitySubmissions']==0
browser.post('+ Add Practical Activity')
browser.post('Save Changes',{'txtTitle':'New Activity','txtInstructions':'Cook again'})
visit('quiz')
browser.post('Delete Quiz')
browser.post('Delete Permanently')
state=inspect()
assert state['Quizzes']==state['QuizQuestions']==state['QuizOptions']==state['QuizAttempts']==0
browser.post('+ Add Quiz')
browser.post('Save Quiz',{'txtQuizTitle':'New Quiz','txtPassingScore':'60'})
browser.post('+ Add Question')
form=Form(browser.html)
option_names=[name for name in form.values if name.endswith('txtOptionText')]
browser.post('Save Question',{'txtQuestionText':'Ready?',option_names[0]:'Yes',option_names[1]:'No'})
assert 'Please select one correct answer.' in browser.html
form=Form(browser.html)
radio=next(r for r in form.inputs if r.get('type')=='radio')
browser.post('Save Question',radio=radio)
assert 'Question saved successfully.' in browser.html
visit('lessons',3)
assert 'No lessons have been added' in browser.html
browser.post('+ Add Lesson')
browser.post('Save Lesson',{'txtTitle':'New Lesson','txtInstructions':'Step one'})
assert 'Lesson added successfully.' in browser.html
visit('overview',3)
browser.post('Submit for Review')
assert 'Before submitting' in browser.html
visit('overview',1)
browser.post('Submit for Review')
assert 'Course submitted for review.' in browser.html
assert not any(b.get('value')=='Delete Course' for b in Form(browser.html).buttons)
fixture('status=Published')
visit()
browser.post('Delete Course')
assert 'learner quiz attempts' in browser.html and 'Reviews and course reports' in browser.html
browser.post('Delete Course Permanently')
state=inspect()
assert state['Courses']==2 and state['Lessons']==1
for table in ['CourseRecipes','Quizzes','QuizQuestions','QuizOptions','QuizAttempts','PracticalActivities','ActivitySubmissions','CourseProgress','Reviews','CourseReports']:
    assert state[table]==0,(table,state)
fixture('learner=1')
assert_denied(PAGE)
print('PASS: all tabs, ownership, status races, edits preserve IDs/history, lesson ordering, pagination, options validation, empty states, resubmission and cascade deletion.')
