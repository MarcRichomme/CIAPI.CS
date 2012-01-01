﻿using CIAPI.IntegrationTests.Streaming;
using CIAPI.Rpc;
using CityIndex.JsonClient;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace CIAPI.IntegrationTests.Rpc
{
    [TestFixture]
    public class NewsFixture : RpcFixtureBase
    {

        [Test(Description = "This test duplicates issue http://faq.labs.cityindex.com/questions/listnewsheadlineswithsource-api-returning-an-error-for-source-mni-and-category-all. When it fails the bug is fixed.")]
        [ExpectedException(typeof(ApiException))]
        public void ListNewsHeadlinesWithSourceMNI()
        {
            var rpcClient = BuildRpcClient();
            var headlines = rpcClient.News.ListNewsHeadlinesWithSource("MNI", "ALL", 100);

            
            
        }
        [Test]
        public void HowToUseNews()
        {
            var rpcClient = BuildRpcClient();




            // get some headlines
            var headlines = rpcClient.News.ListNewsHeadlinesWithSource("dj", "UK", 100);

            // get a story id from one of the headlines
            var storyId = headlines.Headlines[0].StoryId;

            // get the body of the story
            // the api team has yet again broken an established and published interface with yet another
            // half baked change. if you are going to require a key to retrieve detail you need to provide
            // the key on the master. again, the client is required to maintain corellation information (i.e. market type)
            // not impressed.
            var storyDetail = rpcClient.News.GetNewsDetail("dj",storyId.ToString());

            Assert.IsNotNullOrEmpty(storyDetail.NewsDetail.Story, "story was empty?");


            rpcClient.LogOut();
            rpcClient.Dispose();
        }
        [Test]
        public void CanListNewsHeadlinesWithSource()
        {
            var rpcClient = BuildRpcClient();




            // get some headlines
            var headlines = rpcClient.News.ListNewsHeadlinesWithSource("dj","UK", 100);

            // get a story id from one of the headlines
            var storyId = headlines.Headlines[0].StoryId;

            // get the body of the story
            var storyDetail = rpcClient.News.GetNewsDetail("dj", storyId.ToString());
            

            Assert.IsNotNullOrEmpty(storyDetail.NewsDetail.Story, "story was empty?");


            rpcClient.LogOut();
            rpcClient.Dispose();
        }
    }

 
}