using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using FeedReader;
using FeedReader.Xml.Interfaces;

namespace FeedReader.Xml
{
    internal class FeedRdfImageXmlParser : IFeedTypeImageXmlParser
    {
        private Image _feedImage;
        private string _feedImagePath = string.Empty;
        private string _feedImageUrl = string.Empty;

        public Image GetParsedImage()
        {
            return _feedImage;
        }

        public string GetImagePath()
        {
            return _feedImagePath;
        }

        public string GetImageUrl()
        {
            return _feedImageUrl;
        }

        public bool TryParseFeedImageUrl(XDocument xmlFeed, string cacheFolder, string feedTitle, bool bUrlOnly = false)
        {
            bool returnValue = false;
            _feedImage = null;
            _feedImagePath = string.Empty;

            //1. Try
            this._feedImageUrl = FeedXmlParser.ParseString(xmlFeed.Descendants("channel").Elements("image").Attributes("resource"), "channel/image/attribute[resource]");
            if (bUrlOnly)
            {
                if (Uri.IsWellFormedUriString(this._feedImageUrl, UriKind.Absolute))
                    return true;
            }
            else
            {
                returnValue = DownloadAndCheckFeedImageValid(cacheFolder, this._feedImageUrl, feedTitle);
                if (returnValue)
                    return true;
            }

            LogEvents.InvokeOnDebug(new FeedArgs("No feed image could be parsed"));
            return false;
        }

        public bool TryParseFeedItemImageUrl(XDocument xmlFeed, string cacheFolder, string feedTitle, string feedItemTitle, int itemNumber, bool bUrlOnly = false)
        {
            XElement item = xmlFeed.Descendants("item").ElementAt(itemNumber);
            bool returnValue = false;
            _feedImage = null;
            _feedImagePath = string.Empty;
            if (item != null)
            {
                //1. Try
                this._feedImageUrl = FeedXmlParser.ParseString(item.Element("item"), "about", "item[" + itemNumber + "]/attribute[about]");
                if (Path.GetExtension(this._feedImageUrl) == ".gif" || Path.GetExtension(this._feedImageUrl) == ".bmp" ||
                    Path.GetExtension(this._feedImageUrl) == ".jpg" || Path.GetExtension(this._feedImageUrl) == ".png" ||
                    Path.GetExtension(this._feedImageUrl) == ".jpeg")
                {
                    if (bUrlOnly)
                        return true;

                    returnValue = DownloadAndCheckFeedItemImageValid(cacheFolder, this._feedImageUrl, feedTitle, feedItemTitle);
                    if (returnValue)
                        return true;
                }

                //Last Try
                LogEvents.InvokeOnDebug(new FeedArgs("Try to get feed item image out of description field..."));
                string description = FeedXmlParser.ParseString(item.Element("description"), "item[" + itemNumber + "]/description");

                this._feedImageUrl = Utils.GetImageOutOfDescription(Utils.ReplaceHTMLSpecialChars(description));
                if (bUrlOnly)
                {
                    if (Uri.IsWellFormedUriString(this._feedImageUrl, UriKind.Absolute))
                        return true;
                }
                else
                {
                    returnValue = DownloadAndCheckFeedItemImageValid(cacheFolder, this._feedImageUrl, feedTitle, feedItemTitle);
                    if (returnValue)
                        return true;
                }
            }
            LogEvents.InvokeOnDebug(new FeedArgs("No feed item image could be parsed"));
            return false;
        }
        public bool DownloadAndCheckFeedImageValid(string cacheFolder, string url, string feedTitle)
        {
            if (Utils.IsValidUrl(ref url))
            {
                LogEvents.InvokeOnDebug(new FeedArgs("Parsed feed image \"" + url + "\" successfull. Downloading image/Load image from cache..."));
                if (string.IsNullOrEmpty(cacheFolder))
                {
                    if (Utils.LoadFeedImage(url, feedTitle, out _feedImage)) return true;
                }
                else
                {
                    if (Utils.LoadFeedImage(url, feedTitle, cacheFolder, out _feedImage, out _feedImagePath)) return true;
                }
            }
            return false;
        }



        public bool DownloadAndCheckFeedItemImageValid(string cacheFolder, string url, string feedTitle, string feedItemTitle)
        {
            if (Utils.IsValidUrl(ref url))
            {
                LogEvents.InvokeOnDebug(new FeedArgs("Parsed feed item image \"" + url + "\" successfull. Downloading image/Load image from cache..."));
                if (string.IsNullOrEmpty(cacheFolder))
                {
                    if (Utils.LoadFeedItemImage(url, feedTitle, feedItemTitle, out _feedImage)) return true;
                }
                else
                {
                    if (Utils.LoadFeedItemImage(url, feedTitle, feedItemTitle, cacheFolder, out _feedImage, out _feedImagePath)) return true;
                }
            }
            return false;
        }


        public bool TryParseFeedImageUrl(XDocument xmlFeed, string feedTitle, bool bUrlOnly = false)
        {
            return TryParseFeedImageUrl(xmlFeed, string.Empty, feedTitle, bUrlOnly);
        }

        public bool TryParseFeedItemImageUrl(XDocument xmlFeed, string feedTitle, string feedItemTitle, int itemNumber, bool bUrlOnly = false)
        {
            return TryParseFeedItemImageUrl(xmlFeed, string.Empty, feedTitle, feedItemTitle, itemNumber, bUrlOnly);
        }
    }
}
