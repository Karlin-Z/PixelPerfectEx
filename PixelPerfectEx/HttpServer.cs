using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PiPiPlugin.PluginModule;
using PixelPerfectEx.Drawing;
using PixelPerfectEx.Drawing.Types;
using PixelPerfectEx.IPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using static PixelPerfectEx.Drawing.IDrawData;

namespace PixelPerfectEx
{
    public delegate void OnExceptionEventHandler(Exception ex);

    internal class HttpServer
    {
        private Thread _serverThread;
        private HttpListener _listener;

        public int Port { get; private set; }

        public Action<string, string> PPexHttpActionDelegate = null;
        public event OnExceptionEventHandler OnException;

        #region Init
        /// <summary>
        ///     在指定端口启动监听
        /// </summary>
        /// <param name="port">要启动的端口</param>
        public HttpServer(int port)
        {
            Initialize(port);
        }


        /// <summary>
        ///     初始化并启动监听
        /// </summary>
        /// <param name="port">监听的端口</param>
        private void Initialize(int port)
        {
            Port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }

        /// <summary>
        ///     停止监听并释放资源
        /// </summary>
        public void Stop()
        {
            MethodInfo abort = null;

            _serverThread.Interrupt();
            _listener.Stop();
        }
        #endregion


        private void Listen()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");
                _listener.Start();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
                return;
            }

            ThreadPool.QueueUserWorkItem(o => {
                try
                {
                    while (_listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c => {
                            if (!(c is HttpListenerContext context))
                                throw new ArgumentNullException(nameof(context));
                            try
                            {
                                DoAction(ref context);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message);
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                            finally
                            {
                                context.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                }
                catch
                {
                    // ignored
                }
            });
        }

        /// <summary>
        ///     根据HTTP请求内容执行对应的指令
        /// </summary>
        /// <param name="context">HTTP请求内容</param>
        private void DoAction(ref HttpListenerContext context)
        {
            var payload = new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();

            PPexHttpActionDelegate?.Invoke(TrimUrl(context.Request.Url.AbsolutePath), payload);

            var buf = Encoding.UTF8.GetBytes(payload);
            context.Response.ContentLength64 = buf.Length;
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Flush();
        }
        public string TrimUrl(string url)
        {
            return url.Trim(new char[] { '/' });
        }


    }


    public static class NetHandler
    {
        public static void CommandHandler(string command, string args)
        {
            try
            {
                if (command == "DrawAoe")
                {

                    if (Service.Configuration.ShowReceive)
                    {
                        PluginLog.Debug($"receive {args}");
                    }
                    if (args[0] == '[')
                    {
                        List<AOEInfo> actionlist = JsonConvert.DeserializeObject<List<AOEInfo>>(args);

                        DoAction(actionlist);
                    }
                    if (args[0] == '{')
                    {
                        AOEInfo action = JsonConvert.DeserializeObject<AOEInfo>(args);
                        List<AOEInfo> actionlist = new();
                        actionlist.Add(action);
                        DoAction(actionlist);

                    }
                    if (args.StartsWith("Draw"))
                    {
                        var js = args.Substring(4, args.Length - 4);
                        var jo = JObject.Parse(js);
                        if (jo.TryGetValue("AoeType", out var _at))
                        {
                            lock (Service.DrawDatas)
                            {
                                switch (_at.ToObject<AoeTypeEnum>())
                                {

                                    case AoeTypeEnum.Circle:
                                        Service.DrawDatas.Add(new Circle(jo));
                                        break;
                                    case AoeTypeEnum.Donut:
                                        Service.DrawDatas.Add(new Donut(jo));
                                        break;
                                    case AoeTypeEnum.Rect:
                                        Service.DrawDatas.Add(new Rect(jo));
                                        break;
                                    case AoeTypeEnum.Sector:
                                        Service.DrawDatas.Add(new Sector(jo));
                                        break;
                                    case AoeTypeEnum.Repel:
                                        break;
                                    case AoeTypeEnum.Back:
                                        break;
                                    case AoeTypeEnum.TP:
                                        break;
                                    case AoeTypeEnum.Link:
                                        break;
                                    case AoeTypeEnum.Cros:
                                        break;
                                    default:
                                        break;
                                }
                            }


                        }


                    }


                }
                if (command == "Add")
                {
                    if (Service.Configuration.ShowReceive)
                    {
                        PluginLog.Debug($"receive {args}");
                    }

                    if (args[0] == '{')
                    {

                        var jo = JObject.Parse(args);

                        if (jo.TryGetValue("AoeType", out var _at))
                        {
                            lock (Service.DrawDatas)
                            {
                                switch (_at.ToObject<AoeTypeEnum>())
                                {

                                    case AoeTypeEnum.Circle:
                                        Service.DrawDatas.Add(new Circle(jo));
                                        break;
                                    case AoeTypeEnum.Donut:
                                        Service.DrawDatas.Add(new Donut(jo));
                                        break;
                                    case AoeTypeEnum.Rect:
                                        Service.DrawDatas.Add(new Rect(jo));
                                        break;
                                    case AoeTypeEnum.Sector:
                                        Service.DrawDatas.Add(new Sector(jo));
                                        break;
                                    case AoeTypeEnum.Repel:
                                        Service.DrawDatas.Add(new Repel(jo));
                                        break;
                                    case AoeTypeEnum.Back:
                                        Service.DrawDatas.Add(new Back(jo));
                                        break;
                                    case AoeTypeEnum.TP:
                                        break;
                                    case AoeTypeEnum.Link:
                                        Service.DrawDatas.Add(new Link(jo));
                                        break;
                                    case AoeTypeEnum.Cros:
                                        break;
                                    default:
                                        break;
                                }

                            }

                        }

                    }
                }
                if (command == "Remove")
                {
                    if (Service.Configuration.ShowReceive)
                    {
                        PluginLog.Debug($"receive Remove:{args}");
                    }
                    lock (Service.DrawDatas)
                    {
                        if (args == "RemoveAll")
                        {
                            Service.DrawDatas.Clear();
                        }
                        else
                        {
                            Service.DrawDatas.RemoveWhere(dd => dd.Name == args);
                        }
                    }

                }
                if (command == "GetData")
                {
                    if (args == "Team")
                    {
                        PartySort.SendSortedPartyListToAct();
                        PluginLog.Debug("GetTeamSort");
                    }
                }
                if (command == "SetFace")
                {
                    var jo = JObject.Parse(args);
                    if (jo.TryGetValue("Face", out var Face) && jo.TryGetValue("Delay", out var Delay) && jo.TryGetValue("During", out var During))
                    {
                        PiPiPlugin.PluginModule.ObjectFacingHack.SetTo(Face.ToObject<float>(), Delay.ToObject<float>(), During.ToObject<float>());
                        PluginLog.Debug($"Receive Facing {Face.ToObject<float>()} Delay:{Delay.ToObject<float>()} During:{During.ToObject<float>()}");
                    }

                }
                if (command == "Target")
                {
                    var jo = JObject.Parse(args);
                    if (jo.TryGetValue("Type", out var t) && jo.TryGetValue("Id", out var Id) && jo.TryGetValue("Name", out var Name))
                    {
                        if (t.ToObject<int>() == 0)
                        {
                            var id = Id.ToObject<uint>();
                            var obj = Service.GameObjects.SearchById(id);
                            if (obj != null)
                            {
                                Marshal.StructureToPtr<nint>(obj.Address, Service.Address.PlayerTargetPtr, true);
                            }
                        }
                        if (t.ToObject<int>() == 1)
                        {
                            var name = Name.ToObject<string>();
                            var obj = Service.GameObjects.Where((o) => o.Name.ToString() == name).FirstOrDefault();
                            if (obj != null)
                            {
                                Marshal.StructureToPtr<nint>(obj.Address, Service.Address.PlayerTargetPtr, true);
                            }
                        }
                        PluginLog.Debug($"Receive Target {args}");
                    }


                }
                if (command == "Move")
                {
                    if (args == "Clear")
                        AutoMoveHack.ClearAll();
                    else
                    {
                        Vector3 endpos = JsonConvert.DeserializeObject<Vector3>(args);
                        PluginLog.Debug($"receive move {endpos}");
                        AutoMoveHack.WayPoints.Enqueue(endpos);
                    }

                }
            }
            catch (Exception ex)
            {
                Dalamud.Logging.PluginLog.Error($"{ex.Message}|\n{ex.StackTrace}\n{args}");
            }
            
        }

        private static void DoAction(List<AOEInfo> actionlist)
        {
            if (actionlist.Count <= 0)
                return;
            lock (Service.Plugin.AoeInfos)
            {
                foreach (var action in actionlist)
                {
                    action.StartTime = DateTime.Now.AddSeconds(action.Delay);
                    action.EndTime = action.StartTime.AddSeconds(action.During);

                    Service.Plugin.AoeInfos.Add(action);
                }
            }
            
        }

        

    }
}
