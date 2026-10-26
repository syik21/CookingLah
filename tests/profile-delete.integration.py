"""Real HTTP/SQL profile closure checks; run only in the disposable test harness."""
import urllib.request, urllib.parse, http.cookiejar, sys, json, time
from html.parser import HTMLParser
base=sys.argv[1]
page=base+'/Pages/Shared/Profile/ProfilePage.aspx'
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
class Hidden(HTMLParser):
    def __init__(self, html):
        super().__init__(); self.values={}; self.feed(html)
    def handle_starttag(self, tag, attrs):
        a=dict(attrs)
        if tag=='input' and a.get('type')=='hidden' and a.get('name'):
            self.values[a['name']]=a.get('value','')
def get(url):
    try: return client.open(url).read().decode('utf-8-sig')
    except urllib.error.HTTPError as error:
        print(error.read().decode('utf-8-sig')[:6000]); raise
def post(html, **fields):
    data=Hidden(html).values
    data.update(profileAction='delete',deletePassword='test-password',confirmDeleteProfile='yes')
    data.update(fields)
    return client.open(page,urllib.parse.urlencode(data).encode()).read().decode('utf-8-sig')
def state(): return json.loads(get(base+'/Fixture.aspx?inspect=1'))
for attempt in range(60):
    try: get(base+'/Fixture.aspx?id=4'); break
    except Exception:
        if attempt==59: raise
        time.sleep(.5)
html=get(page)
assert 'id="openDeleteProfile"' in html
html=post(html,profileCsrf='wrong')
assert 'Your session has changed' in html and '4' in state()
html=post(html,confirmDeleteProfile='')
assert 'Please confirm that you understand' in html and '4' in state()
html=post(html,deletePassword='incorrect')
assert 'Your current password is incorrect' in html and '4' in state()
post(html)
assert '4' not in state(), 'Learner was not deleted'
assert 'id="openDeleteProfile"' not in get(page), 'Deleted user session still active'
get(base+'/Fixture.aspx?id=1'); html=get(page)
assert 'Deactivate profile' in html
before=state()['courses']
post(html)
assert state()['1']=='Inactive' and state()['courses']==before
get(base+'/Fixture.aspx?id=3'); html=get(page)
assert 'id="openDeleteProfile"' not in html
html=post(html)
assert 'This account cannot be closed here' in html and state()['3']=='Active'
print('PASS: CSRF, confirmation, wrong password, learner deletion/sign-out, chef deactivation/course retention, admin self-removal blocked.')
