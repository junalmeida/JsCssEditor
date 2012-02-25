// 
// JsCssParsedDocument.cs
//  
// Author:
//       Marcos Almeida Junior <junalmeida@gmail.com>
// 
// Copyright (c) 2012 Marcos Almeida Junior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using MonoDevelop.Projects.Dom;
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.Core;

namespace JsCssEditor
{
    public class JsCssParsedDocument : ParsedDocument
    {
 
        private string[] commentPairs;
        private string fileContent;
        private string lineEnding;
        private string[] regionPairs;
        private List<int> usedChars;
     
        public JsCssParsedDocument (string fileName, string fileContent, string[] regionPairs, string[] commentPairs) : base(fileName)
        {
            this.lineEnding = "";
            
            if (fileContent.Contains ("\r"))
                this.lineEnding = this.lineEnding + "\r";
            if (fileContent.Contains ("\n"))
                this.lineEnding = this.lineEnding + "\n";
            if (this.lineEnding == "")
                this.lineEnding = Environment.NewLine;
        
            this.fileContent = fileContent + this.lineEnding + this.lineEnding;
            this.usedChars = new List<int> ();
            this.regionPairs = regionPairs;
            this.commentPairs = commentPairs;
        }

        public int FindComment (int startIndex, List<FoldingRegion> regions, string startText, string endText)
        {
            return -1;
        }

        public int FindRegion (int lastIndex, List<FoldingRegion> regions, string startText, string endText)
        {
            try {
                var startChar = this.fileContent.LastIndexOf (startText, lastIndex);
                if (!this.usedChars.Contains (startChar)) {
                    var endChar = startChar;
                    
                    do {
                        endChar = this.fileContent.IndexOf (endText, endChar + 1);
                    } while (this.usedChars.Contains(endChar));
                    
                    if (endChar > startChar) {
                        var regionName = this.fileContent.Substring (startChar + startText.Length, 
                                                                 this.fileContent.IndexOf (this.lineEnding, startChar) - (startChar + startText.Length)).Trim ();
                        if (regionName.EndsWith ("*/")) {
                            regionName = regionName.Substring (0, regionName.IndexOf ("*/")).Trim ();
                        }
                        var startLine = this.fileContent.Substring (0, startChar).Count (chr => chr == lineEnding [0]) + 1;
                        var endLine = this.fileContent.Substring (startChar, endChar - startChar).Count (chr => chr == lineEnding [0]) + startLine + 1;
                    
                        regions.Add (new FoldingRegion (regionName, new DomRegion (startLine, endLine), FoldType.UserRegion));
                        this.usedChars.Add (startChar);
                        this.usedChars.Add (endChar);
                    }
                }
                return startChar;
            } catch (Exception exception) {
                LoggingService.LogError ("Error in JsCssEditor: " + exception.GetType ().Name, exception);
                return -1;
            }
        }

        public override IEnumerable<FoldingRegion> GenerateFolds ()
        {
            this.usedChars.Clear ();
            var regions = new List<FoldingRegion> (base.GenerateFolds ());
            
            if (this.regionPairs != null) {
                for (int i = 0; i < this.regionPairs.Length; i += 2) {
                    for (int j = this.fileContent.Length; j > -1; j = this.FindRegion(j, regions, this.regionPairs[i], this.regionPairs[i + 1])) {
                    }
                }
            }
            if (this.commentPairs != null) {
                for (int k = 0; k < this.commentPairs.Length; k += 2) {
                    for (int m = this.fileContent.Length; m > -1; m = this.FindComment(m, regions, this.commentPairs[k], this.commentPairs[k + 1])) {
                    }
                }
            }
            return regions;
        }
    }

  

}

