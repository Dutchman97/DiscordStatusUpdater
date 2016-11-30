﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordStatusUpdater.Players
{
    public class TitleParser
    {
        Regex regex;
        int captIdx;

        public TitleParser(string prefix, string suffix)
        {
            // Regex to capture player prefix and suffix, and file extension, and video file name
            regex = new Regex(@"^(" + prefix + @")(.+)(\.\w{1,4})(" + suffix + ")$");
            captIdx = 2;
        }

        public bool TryParse(string fullTitle, out string result)
        {
            // First, check if the title can be parsed
            if (!regex.IsMatch(fullTitle))
            {
                result = null;
                return false;
            }

            // Get the captured video file name using the regex
            // Note: for some reason, group index starts with 1, while capture index starts with 0
            Match match = regex.Match(fullTitle);
            GroupCollection groups = match.Groups;
            Group group = groups[captIdx];
            CaptureCollection captures = group.Captures;
            Capture capture = captures[0];
            string videoName = capture.Value;

            // For internet-downloaded files, replace underscores with whitespace
            if (!videoName.Contains(" "))
                videoName = videoName.Replace('_', ' ');

            // Remove everything between square brackets
            string bracketsRemoved = Regex.Replace(videoName, @"\[.+?\]", "");
            if (bracketsRemoved == string.Empty)
                // For certain (sub)groups that put everything in square brackets *cough* philosophy-raws *cough*
                videoName = bracketsRemoved;
            else if (bracketsRemoved == videoName)
                // For certain (sub)groups that use round brackets instead of square brackets *cough* Grey_Phantom *cough*
                videoName = Regex.Replace(videoName, @"\(.+?\)", "");

            // Replace multiple whitespace characters after each other with a single whitespace character
            videoName = Regex.Replace(videoName, @"\s+", " ");

            // Remove all leading and trailing whitespace
            videoName = videoName.Trim();
            result = videoName;
            return true;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }
}
