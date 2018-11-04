﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DEV_4
{
    /// <summary>
    /// Class XmlParser
    /// Parse XML string.
    /// </summary>
    public class XmlParser
    {
        
        private List<string> ParsedResult;
        private FlagsOfTheState flagsOfTheState;  
        private string XmlString { get; set; }
        private IXmlTag XmlTag { get; set; }
        private ReadyArgument Argument { get; }
        private StringBuilder AddString { get; }
        // Stack of open tags.
        private Stack<string> StackWithTags { get; }
        
        public string XmlAddress { get; set; }
        
        public XmlParser(string receivedString)
        {
            ParsedResult = new List<string>();
            XmlAddress = receivedString;
            flagsOfTheState = new FlagsOfTheState();
            AddString = new StringBuilder();
            StackWithTags = new Stack<string>();
            Argument = new ReadyArgument(StackWithTags, ParsedResult, flagsOfTheState);
        }

        /// <summary>
        /// Method Parsing
        /// Gets the address of the XML file and starts ParsingXML method.
        /// </summary> 
        /// <returns>Parsed list of strings.</returns>
        public List<string> Parsing()
        {
            if (XmlAddress == null)
            {
                throw new Exception("Empty address.");
            }
            // Convert the file to a string.
            var XmlToStringConverter = new FileToStringConverter(XmlAddress);
            XmlString = XmlToStringConverter.ReturnedString;
            List<string> ParsedResult = ParsingXml();

            return ParsedResult;
        }
        
        /// <summary>
        /// Method ParsingXml
        /// Parsing a string, if the XML file is compiled
        /// correctly, returns the result of the parsing.
        /// </summary>
        private List<string> ParsingXml()
        {   
            for (var i = 0; i < XmlString.Length; i++)
            {
                SkipComment(ref i);

                // Skip character if it is a space or a line break.
                if (((XmlString[i] == ' ') && (!flagsOfTheState.TagFlag && !flagsOfTheState.ArgumentFlag)) || (XmlString[i] == '\n'))
                {
                    continue;
                }

                if (XmlString[i] == '<')
                {
                    // Check for opening tag one more time.
                    if (flagsOfTheState.TagFlag)
                    {
                        throw new Exception("Incorrect brackets.");
                    }

                    // If there is a ready argument, then write it down.
                    Argument.CreateArgument(AddString.ToString());
                    AddString.Clear();
                    GetTypeOfTag(XmlString, flagsOfTheState, ref i);
                    
                    continue;
                }


                if (XmlString[i] == '>')
                {
                    // Check for closing only open tag.
                    if (!flagsOfTheState.TagFlag)
                    {
                        throw new Exception("Incorrect brackets.");
                    }
                    else
                    {
                        XmlTag = new XmlTag(StackWithTags,AddString.ToString());
                    }
                    
                    // Check for XML declaration at the beginning.
                    if (!flagsOfTheState.XmlFlag)
                    {
                        XmlTag = new XmlDeclarationTag(flagsOfTheState, AddString.ToString());
                    }
                    
                    SkipDoctype();
                    
                    // If it is a closing tag, it checks for consistency with the tags in the stack.
                    if (flagsOfTheState.ClosingTagFlag)
                    {
                        XmlTag = new ClosingXmlTag(StackWithTags, AddString.ToString());
                    }
                    
                    // If this is an empty tag. (< ... />)
                    if (XmlString[i - 1] == '/')
                    {
                        XmlTag = new EmptyXmlTag(Argument, AddString.ToString());
                    }
                    
                    XmlTag.Implemet();
                    flagsOfTheState.DisableParsingTag();
                    AddString.Clear();
                    
                    continue;
                }

                if (!flagsOfTheState.TagFlag)
                {
                    flagsOfTheState.ArgumentFlag = true;
                }

                AddString.Append(XmlString[i]);
            }

            if (StackWithTags.Count != 0)
            {
                throw new Exception("Incorrectly closed tags.");
            }

            return ParsedResult;
        }
        
        /// <summary>
        /// Method GetTypeOfTag
        /// Gives type tag.
        /// </summary>
        /// <param name="XmlString">XML file is translated into a string.</param>
        /// <param name="flagsOfTheState">The current state of the parser.</param>
        /// <param name="index">The index of the character being processed.</param>
        public void GetTypeOfTag(string XmlString,FlagsOfTheState flagsOfTheState, ref int index)
        {
            flagsOfTheState.TagFlag = true;
            // Separates the tag from the closing tag.
            if (XmlString[index + 1] == '/')
            {
                index++;
                flagsOfTheState.ClosingTagFlag = true;
            }
                    
            // Separates the tag from the comment.
            if ((XmlString[index + 1] == '!') && (XmlString[index + 2] == '-') && (XmlString[index + 3] == '-'))
            {
                index += 3; // Skipping, the character of the start of the comment (<!--).
                flagsOfTheState.CommentFlag = true;
                flagsOfTheState.TagFlag = false;
            }
        }
        
        /// <summary>
        /// Method SkipComment
        /// Moves the index of the character being processed to the end of the comment.
        /// </summary>
        /// <param name="index">The index of the character being processed.</param>
        private void SkipComment(ref int index)
        {
            while (flagsOfTheState.CommentFlag)
            {
                if (index+2 > XmlString.Length)
                {
                    throw new Exception("Comment is not closed.");
                }
                    
                // Search end of comment
                if ((XmlString[index] == '-') && (XmlString[index+1] == '-') && (XmlString[index+2] == '>'))
                {
                    index += 2; // Skipping, the character of the end of the comment(-->).
                    flagsOfTheState.CommentFlag = false;
                }

                index++;
            }
        }
        
        public void SkipDoctype()
        {
            if (AddString.ToString().Contains("!DOCTYPE"))
            {
                AddString.Clear();
                flagsOfTheState.DisableParsingTag();
            }
        }
    }
}