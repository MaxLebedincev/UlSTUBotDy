using HandlerUlSTU.DTO;
using System.Net;

namespace HandlerUlSTU
{
    public class InteractionAPI
    {
        #region Учетные данные
        private string _login { get; set; } = "";
        private string _password { get; set; } = "";
        public string Login
        {
            get { return _login; }
            private set { _login = value; }
        }
        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }

        public void UpdateСredential(string login, string password)
        {
            _login = (login == null) ? throw new Exception("") : login;
            _password = (password == null) ? throw new Exception("") : password;
        }
        public void UpdateСredential(Credential credential)
        {
            _login = (credential.Login == null) ? throw new Exception("") : credential.Login;
            _password = (credential.Password == null) ? throw new Exception("") : credential.Password;
        }
        #endregion

        #region Точки входа
        private string _authorizationUrl { get; set; }
        private string _timetablePrivateUrl { get; set; }
        private string _allFilesPrivateUrl { get; set; }

        public string AuthorizationUrl
        {
            get { return _authorizationUrl; }
            private set { _authorizationUrl = value; }
        }
        public string TimetablePrivateUrl
        {
            get { return _timetablePrivateUrl; }
            private set { _timetablePrivateUrl = value; }
        }
        public string AllFilesPrivateUrl
        {
            get { return _allFilesPrivateUrl; }
            private set { _allFilesPrivateUrl = value; }
        }
        #endregion
        
        public InteractionAPI(EntryPoint points)
        {
            _authorizationUrl = String.IsNullOrEmpty(points.Authorization) ? throw new Exception("") : points.Authorization;
            _timetablePrivateUrl = String.IsNullOrEmpty(points.TimetablePrivate) ? throw new Exception("") : points.TimetablePrivate;
            _allFilesPrivateUrl = String.IsNullOrEmpty(points.AllFilesPrivate) ? throw new Exception("") : points.AllFilesPrivate;
        }

        public InteractionAPI(Credential credential, EntryPoint points) : this (points)
        {
            _login = (credential.Login == null) ? throw new Exception("") : credential.Login;
            _password = (credential.Password == null) ? throw new Exception("") : credential.Password;

            _authorizationUrl = String.IsNullOrEmpty(points.Authorization) ? throw new Exception("") : points.Authorization;
            _timetablePrivateUrl = String.IsNullOrEmpty(points.TimetablePrivate) ? throw new Exception("") : points.TimetablePrivate;
            _allFilesPrivateUrl = String.IsNullOrEmpty(points.AllFilesPrivate) ? throw new Exception("") : points.AllFilesPrivate;
        }

        public async Task<IEnumerable<Cookie>> Authorization()
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            HttpClient client = new HttpClient(handler);

            HttpResponseMessage response = await client.PostAsync(AuthorizationUrl, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("login", Login),
                new KeyValuePair<string, string>("password", Password)
            }));

            return cookies.GetCookies(new Uri(AuthorizationUrl)).Cast<Cookie>();
        }

        public async Task<IEnumerable<Cookie>> Authorization(string login, string password)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            HttpClient client = new HttpClient(handler);

            await client.PostAsync(AuthorizationUrl, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("login", login ?? throw new Exception("")),
                new KeyValuePair<string, string>("password", password ?? throw new Exception(""))
            }));

            return cookies.GetCookies(new Uri(AuthorizationUrl)).Cast<Cookie>();
        }

        private HttpClient CreatePrivateClient(IEnumerable<Cookie> cookies)
        {
            CookieContainer authCookies = new CookieContainer();

            foreach (Cookie cookie in cookies)
            {
                authCookies.Add(cookie);
            }

            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = authCookies;

            return new HttpClient(handler);
        }

        public async Task<string> GetJsonTimetable(IEnumerable<Cookie> cookies, string? nameGroup)
        {
            HttpClient client = CreatePrivateClient(cookies);

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(TimetablePrivateUrl + "?filter=" + nameGroup);
            }
            catch(Exception ex) { throw new Exception(ex.Message); }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetHtmlPageWithAllFiles(IEnumerable<Cookie> cookies)
        {
            HttpClient client = CreatePrivateClient(cookies);

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(AllFilesPrivateUrl);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetJsonTimetableWithAuth(string login, string password, string? nameGroup)
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(login, password);
            }
            catch(Exception ex) { throw new Exception(ex.Message); }

            string answer = "";

            try
            {
                answer = await GetJsonTimetable(authCookies, nameGroup);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }

        public async Task<string> GetJsonTimetableWithAuth(string? nameGroup)
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(_login, _password);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            string answer = "";

            try
            {
                answer = await GetJsonTimetable(authCookies, nameGroup);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }

        public async Task<string> GetHtmlPageWithAllFilesWithAuth(string login, string password)
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(login, password);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            string answer = "";

            try
            {
                answer = await GetHtmlPageWithAllFiles(authCookies);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }

        public async Task<string> GetHtmlPageWithAllFilesWithAuth()
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(_login, _password);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            string answer = "";

            try
            {
                answer = await GetHtmlPageWithAllFiles(authCookies);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }

        public async Task<byte[]> GetBytesFile(IEnumerable<Cookie> cookies, string? folderFileName)
        {
            HttpClient client = CreatePrivateClient(cookies);

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync(AllFilesPrivateUrl + folderFileName);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> GetBytesFileWithAuth(string login, string password, string? folderFileName)
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(login, password);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            byte[] answer;

            try
            {
                answer = await GetBytesFile(authCookies, folderFileName);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }

        public async Task<byte[]> GetBytesFileWithAuth(string? folderFileName)
        {
            IEnumerable<Cookie> authCookies;

            try
            {
                authCookies = await Authorization(_login, _password);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            byte[] answer;

            try
            {
                answer = await GetBytesFile(authCookies, folderFileName);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return answer;
        }
    }
}
