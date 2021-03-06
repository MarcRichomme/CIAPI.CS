﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Salient.ReflectiveLoggingAdapter;
using Salient.ReliableHttpClient;

namespace CIAPI.Rpc
{
    public class MetricsRecorder : Recorder
    {
     
        public Uri AppmetricsUri { get; private set; }
        private Timer _metricsTimer;
        
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetricsRecorder));
        
        public MetricsRecorder(Client client,Uri appmetricsUri):base(client)
        {
            AppmetricsUri = appmetricsUri;
            _metricsTimer = new Timer(ignored => PostMetrics(), null, 1000, 10000);
        }

  
        private void PostMetrics()
        {
            
            if(Paused )
            {
                return;
            }


            var records = GetRequests();
            Clear();

            if (records.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var item in records)
            {
                try
                {
                    if (item.Uri.AbsoluteUri != AppmetricsUri.AbsoluteUri)
                    {
                        sb.AppendLine(string.Format("{0}\tLatency {1}\t{2}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), item.Uri, item.Completed.Subtract(item.Issued).TotalSeconds));
                    }

                }
                catch
                {

                    //swallow. this is just a safety swallow. no need to log
                }
            }


            var latencyData = sb.ToString();

            Log.Debug("LATENCY:/n" + latencyData);

            try
            {
                ((Client)Client).BeginRequest(RequestMethod.POST, AppmetricsUri.AbsoluteUri, "", new Dictionary<string, string>(), new Dictionary<string, object> { { "MessageAppKey", ((Client)Client).AppKey ?? "null" }, { "MessageSession", ((Client)Client).Session ?? "null" }, { "MessagesList", latencyData } },
                                     ContentType.FORM,
                                     ContentType.TEXT,
                                     TimeSpan.FromMilliseconds(0),
                                     30000,
                                     0, ar =>
                                            {
                                                try
                                                {
                                                    ((Client)Client).EndRequest(ar);
                                                    Log.Debug("Latency message complete.");
                                                }
                                                catch (Exception ex)
                                                {

                                                    Log.Error("Latency message failed to complete.", ex);
                                                }

                                            }, null);

            }
            catch (Exception ex2)
            {
                Log.Error("Latency message failed to be issued.", ex2);
            }
        }
        public void Dispose()
        {
            try
            {
                if (_metricsTimer != null)
                {
                    _metricsTimer.Dispose();
                    _metricsTimer = null;
                }
            }
            catch
            {

                //swallow
            }

        }
    }
}