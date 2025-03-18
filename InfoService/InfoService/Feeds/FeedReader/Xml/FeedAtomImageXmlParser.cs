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
    class FeedAtomImageXmlParser : IFeedTypeImageXmlParser
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
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
            this._feedImageUrl = null;
            bool returnValue = false;
            _feedImage = null;
            _feedImagePath = string.Empty;

            //1. Try
            this._feedImageUrl = FeedXmlParser.ParseString(xmlFeed.Descendants(atom + "feed").Elements(atom + "logo"), "feed/logo");
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

            //2. Try
            this._feedImageUrl = FeedXmlParser.ParseString(xmlFeed.Descendants(atom + "feed").Elements(atom + "icon"), "feed/icon");
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
            this._feedImageUrl = null;
            bool returnValue = false;
            _feedImage = null;
            _feedImagePath = string.Empty;
            XElement item = xmlFeed.Descendants(atom + "entry").ElementAt(itemNumber);
            if (item != null)
            {
                //1. Try
                string enclosureType = FeedXmlParser.ParseString(item, "type", "entry[" + itemNumber + "]/content/attribute[type]");
                if (Utils.IsEnclosureTypeImage(enclosureType))
                {
                    this._feedImageUrl = FeedXmlParser.ParseString(item.Element(atom + "content"), "src", "entry[" + itemNumber + "]/content/attribute[source]");
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


                //Last Try
                LogEvents.InvokeOnDebug(new FeedArgs("Try to get feed image out of summary field..."));
                string description = FeedXmlParser.ParseString(item.Element(atom + "summary"), "entry[" + itemNumber + "]/summary");

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
