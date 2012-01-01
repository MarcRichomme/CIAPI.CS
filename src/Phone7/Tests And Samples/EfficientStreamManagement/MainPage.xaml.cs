﻿using System;
using System.Threading;
using System.Windows;
using CIAPI.DTO;
using CIAPI.Streaming;
using CityIndex.JsonClient;
using Microsoft.Phone.Controls;
using StreamingClient;
using Client = CIAPI.Rpc.Client;
using IStreamingClient = CIAPI.Streaming.IStreamingClient;

namespace EfficientStreamManagement
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string USERNAME = "XX531040";
        private const string PASSWORD = "password";
        public static Client RpcClient;
        public static IStreamingClient StreamingClient;

        private static readonly Uri RPC_URI = new Uri("https://ciapipreprod.cityindextest9.co.uk/TradingApi");
        private static readonly Uri STREAMING_URI = new Uri("https://pushpreprod.cityindextest9.co.uk");
        private IStreamingListener<ClientAccountMarginDTO> _clientAccountMarginsListener;
        private IStreamingListener<NewsDTO> _newsListener;
        private IStreamingListener<OrderDTO> _orderslistener;
        private IStreamingListener<PriceDTO> _priceListener;


        // Constructor
        public MainPage()
        {
            InitializeComponent();

            BuildClients();
        }

        private void BuildClients()
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("creating rpc client"));
            RpcClient = new Client(RPC_URI);
            RpcClient.BeginLogIn(USERNAME, PASSWORD, InitializeStreamingClient, null);
        }

        public void InitializeStreamingClient(ApiAsyncResult<ApiLogOnResponseDTO> ar)
        {
            RpcClient.EndLogIn(ar);
            StreamingClient = StreamingClientFactory.CreateStreamingClient(STREAMING_URI, RpcClient.UserName,
                                                                           RpcClient.Session);
            Dispatcher.BeginInvoke(() =>
                                       {
                                           listBox1.Items.Add("rpc client logged in");
                                           button1.IsEnabled = true;
                                           button2.IsEnabled = true;
                                       });
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
                           {
                               TearDownListeners();
                               BuildListenerSetOne();
                           }).Start();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
                           {
                               TearDownListeners();
                               BuildListenerSetTwo();
                           }).Start();
        }


        private void TearDownListeners()
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("tearing down all listeners"));

            if (_priceListener != null)
            {
                _priceListener.MessageReceived -= PriceListenerMessageReceived;
                StreamingClient.TearDownListener(_priceListener);
            }

            if (_clientAccountMarginsListener != null)
            {
                _clientAccountMarginsListener.MessageReceived -= ClientAccountMarginsListenerMessageReceived;
                StreamingClient.TearDownListener(_clientAccountMarginsListener);
            }

            if (_newsListener != null)
            {
                _newsListener.MessageReceived -= _newsListener_MessageReceived;
                StreamingClient.TearDownListener(_newsListener);
            }

            if (_orderslistener != null)
            {
                _orderslistener.MessageReceived -= _orderslistener_MessageReceived;
                StreamingClient.TearDownListener(_orderslistener);
            }
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("all listeners torn down"));
        }

        private void BuildListenerSetOne()
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("building listener set one"));
            _orderslistener = StreamingClient.BuildOrdersListener();
            _orderslistener.MessageReceived += _orderslistener_MessageReceived;
            _newsListener = StreamingClient.BuildNewsHeadlinesListener("UK");
            _newsListener.MessageReceived += _newsListener_MessageReceived;
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("listener set one built"));
        }

        private void BuildListenerSetTwo()
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("building listener set two"));
            _priceListener = StreamingClient.BuildPricesListener(71442);
            _priceListener.MessageReceived += PriceListenerMessageReceived;
            _clientAccountMarginsListener = StreamingClient.BuildClientAccountMarginListener();
            _clientAccountMarginsListener.MessageReceived += ClientAccountMarginsListenerMessageReceived;
            Dispatcher.BeginInvoke(() => listBox1.Items.Add("listener set two built"));
        }


        private void _orderslistener_MessageReceived(object sender, MessageEventArgs<OrderDTO> e)
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add(e.Data.MarketId));
        }

        private void _newsListener_MessageReceived(object sender, MessageEventArgs<NewsDTO> e)
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add(e.Data.Headline));
        }

        private void ClientAccountMarginsListenerMessageReceived(object sender,
                                                                 MessageEventArgs<ClientAccountMarginDTO> e)
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add(e.Data.Margin));
        }

        private void PriceListenerMessageReceived(object sender, MessageEventArgs<PriceDTO> e)
        {
            Dispatcher.BeginInvoke(() => listBox1.Items.Add(e.Data.AuditId));
        }
    }
}