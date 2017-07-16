using System;
using System.Threading.Tasks;

using WebSharpJs.Electron;
using WebSharpJs.Script;

//namespace MainWindow
//{
    public class Startup
    {

        static WebSharpJs.NodeJS.Console console;

        static App app;
        static BrowserWindow mainWindow;
        static Session session;
        static int windowId = 0;


        
        /// <summary>
        /// Default entry into managed code.
        /// </summary>
        /// <param name="__dirname">The directory name of the current module. This the same as the path.dirname() of the __filename.</param>
        /// <returns></returns>
        public async Task<object> Invoke(string __dirname)
        {
            if (console == null)
                console = await WebSharpJs.NodeJS.Console.Instance();

            try
            {
                app = await App.Instance();

                // We use app.IsReady instead of listening for the 'ready'event.
                // By the time we get here from the main.js module the 'ready' event has
                // already fired.
                if (await app.IsReady())
                {
                    windowId = await CreateWindow(__dirname);

                    // Get our session
                    try
                    {
                        session = await Session.FromPartition("partition:nameofpartition");
                    }
                    catch (Exception sexc)
                    {
                        await console.Log($"logging: {sexc.Message}");
                        var web = await mainWindow.GetWebContents();
                        session = await web.GetSession();
                    }
                    if (session != null)
                    {

                        await session.SetPermissionRequestHandler(
                            new ScriptObjectCallback<WebContents, string, Func<object, Task<object>>>(

                                async (callbackResult) =>
                                {
                                    var permissionResult = new PermissionRequestResult(callbackResult);

                                    var url = await permissionResult.WebContents.GetURL();
                                    await console.Log($"Received permission request from {url} for access to \"{permissionResult.Permission}\".");
                                    if (permissionResult.Permission == Permission.PointerLock)
                                         permissionResult.Callback(await GrantAccess(permissionResult.Permission));
                                    else
                                        permissionResult.Callback(true);
                                    
                                }
                            )
                        );
                    }
                }


                await console.Log($"Loading: file://{__dirname}/index.html");
            }
            catch (Exception exc) { await console.Log($"extension exception:  {exc.Message}"); }

            return windowId;


        }

        async Task<int> CreateWindow (string __dirname)
        {
            // Create the browser window.
            mainWindow = await BrowserWindow.Create(new BrowserWindowOptions { UseContentSize = true});
                

            // and load the index.html of the app.
            await mainWindow.LoadURL($"file://{__dirname}/index.html");

            // Open the DevTools
            // await mainWindow.GetWebContents().ContinueWith(
            //         (t) => { t.Result?.OpenDevTools(DevToolsMode.Bottom); }
            // );

            return await mainWindow.GetId();

        }

        async Task<bool> GrantAccess(Permission permission)
        {
            var dialog = await Dialog.Instance();
            var options = new MessageBoxOptions() 
            {
                MessageBoxType = MessageBoxType.Question,
                Buttons = new string[] {"Allow PointerLock Access", "Dont't Allow"},
                Title = $"Allow permission?",
                Detail = $"Will you allow this application to lock the mouse pointer?"
                + "  Click the mouse again to unlock the mouse pointer.",
            };
            var result = await dialog.ShowMessageBox(mainWindow, options);
            await console.Log(result);
            if (result == 0)
                return true;
            else
                return false;
        }
    }
//}
