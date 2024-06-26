﻿using Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Helpers;
using System.Xml;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Services
{
    public class ParserService : IParserService
    {
        private static XmlNode _rootNode;

        public List<int> ParseHtmlAndGetRankings(string htmlResponse, string targetUrl)
        {
            XmlDocument document = new();
            try
            {
                document.LoadXml(htmlResponse);
                _rootNode = null;
                SetRootNode(document, Constants.SearchResultsCount);
                if (_rootNode == null)
                {
                    throw new Exception($"The number of search result nodes is less than the expected amount of: {Constants.SearchResultsCount}");
                }

                var rankings = GetSeoRankings(_rootNode, targetUrl);
                return rankings;
            }
            catch (XmlException exception)
            {
                throw new Exception($"Error parsing HTML string from the response: {exception.Message}");
            }
        }

        private static void SetRootNode(XmlNode node, int threshold)
        {
            if (node == null)
            {
                return;
            }

            int directChildCount = node.ChildNodes.Count;
            if (directChildCount >= threshold)
            {
                _rootNode = node;
            }
            else
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    SetRootNode(child, threshold);
                }
            }
        }

        private static List<int> GetSeoRankings(XmlNode rootNode, string targetUrl)
        {
            List<int> notFound = [-1];
            var rankings = new List<int>();
            var currentRank = 0;
            if (rootNode == null)
            {
                return notFound;
            }

            foreach (XmlNode child in rootNode.ChildNodes)
            {
                if (child.Name.Equals("div", StringComparison.OrdinalIgnoreCase))
                {
                    var isSearchResultHeader = FindChildNodeDeep(child, "h3") != null;
                    var isGoogleMapsResult = FindChildNodeDeep(child, "div", "Directions") != null;
                    var isNotASearchResult = FindChildNodeDeep(child, "div", null, "role", "button") != null;
                    if (!isSearchResultHeader || isGoogleMapsResult || isNotASearchResult)
                    {
                        continue;
                    }

                    // Only increase rank if there was an <h3> search result header and it's not a google maps result.
                    currentRank++;
                    if (currentRank > Constants.SearchResultsCount)
                    {
                        break;
                    }

                    var targetUrlFound = FindChildNodeDeep(child, "div", targetUrl);
                    if (targetUrlFound != null)
                    {
                        rankings.Add(currentRank);
                    }
                }
            }

            if (rankings.Any())
            {
                return rankings;
            }
            return notFound;
        }

        private static XmlNode FindChildNodeDeep(XmlNode node, string nodeName, string searchText = null, string attributeName = null, string attributeValue = null)
        {
            if (node == null)
            {
                return null;
            }

            // Check current node first
            if (node.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(nodeName))
            {
                bool textMatch = searchText == null || node.InnerText.Trim().Contains(searchText, StringComparison.OrdinalIgnoreCase);
                bool attributeMatch = attributeName == null ||
                                      (node.Attributes != null &&
                                       node.Attributes[attributeName] != null &&
                                       node.Attributes[attributeName].Value == attributeValue);

                if (textMatch && attributeMatch)
                {
                    return node;
                }
            }

            // Recursively check all children
            foreach (XmlNode child in node.ChildNodes)
            {
                var foundNode = FindChildNodeDeep(child, nodeName, searchText, attributeName, attributeValue);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }
    }
}
