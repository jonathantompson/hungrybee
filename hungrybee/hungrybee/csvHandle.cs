using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace hungrybee
{


    /// <summary>
    /// ***********************************************************************
    /// **                          csvHandle                                **
    /// ** Base class for csvHandleRead and csvHandleWrite                   **
    /// ***********************************************************************
    /// </summary>
    public class csvHandle
    {
        protected string fileName;
        protected bool open;

        public csvHandle()
        {
            open = false;
        }

        // "inline" functions (though not supported in C#)
        public bool IsOpen() { return open; }
        public string GetFileName() { return fileName; }
    }

    /// <summary>
    /// ***********************************************************************
    /// **                          csvHandleRead                            **
    /// ** Reader class to perform fileIO to and from a csv file             **
    /// ***********************************************************************
    /// </summary>
    public class csvHandleRead : csvHandle
    {
        /// <summary>
        /// Local variables
        /// **********************************************************************
        /// </summary>
        private StreamReader reader;

        /// <summary>
        /// Constructor to attempt to open the file.  If file exists, open = true;
        /// **********************************************************************
        /// </summary>
        public csvHandleRead(string fileNameIn)
        {
            fileName = fileNameIn;
            open = false;
            reader = null;
            if (File.Exists(fileName))
            {
                try
                {
                    reader = new StreamReader(fileName); // (Not thread safe)
                    open = true;
                }
                catch (Exception ex_in)
                {
                    open = false;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("csvHandleRead::csvHandleRead(): The file " + fileName + " could not be read"); System.Diagnostics.Debug.Flush();
#endif
                    System.Exception ex_out = new System.Exception("csvHandleRead::csvHandleRead(): Couldn't open StreamReader: " + ex_in.ToString());
                    throw ex_out;
                }
            }
        }

        /// <summary>
        /// Destructor (close the file if it isn't already
        /// **********************************************************************
        /// </summary>
        ~csvHandleRead()
        {
            if (reader != null)
                reader.Close();
        }

        /// <summary>
        /// Destructor (close the file if it isn't already
        /// **********************************************************************
        /// </summary>
        public void Close()
        {
            open = false;
            if (reader != null)
                reader.Close();
        }

        /// <summary>
        /// ReadNextToken: Parse another csv token, return false if we're at the end
        /// **********************************************************************
        /// </summary>
        public bool ReadNextToken(ref List<string> tokenArray)
        {
            if (open == false)
                throw new System.Exception("csvHandleRead::ReadNextToken(): Trying to read from a closed file");

            // Try reading the next line
            string line;
            try
            {
                line = reader.ReadLine();
            }
            catch (Exception ex_in)
            {
                open = false;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("csvHandleRead::ReadNextToken(): The file " + fileName + " could not be read"); System.Diagnostics.Debug.Flush();
#endif
                System.Exception ex_out = new System.Exception("csvHandleRead::ReadNextToken(): Couldn't open StreamReader: " + ex_in.ToString());
                throw ex_out;
            }

            if (line == null)
                return false; // No more lines to read

            // Parse through the string and build up the return array
            tokenArray = new List<string>();
            StringBuilder curToken = new StringBuilder();
            for(int i = 0; i < line.Length; i ++ )
            {
                if (line[i] == ',')
                {
                    tokenArray.Add(curToken.ToString());
                    curToken.Length = 0;  // Fastest way of clearing a string builder
                }
                else
                    curToken.Append(line[i]);
            }
            // Now add the last element
            tokenArray.Add(curToken.ToString());

            return true;
        }
    }

    /// <summary>
    /// ***********************************************************************
    /// **                          csvHandleWrite                           **
    /// ** Reader class to perform fileIO to and from a csv file             **
    /// ***********************************************************************
    /// </summary>
    public class csvHandleWrite : csvHandle
    {
        /// <summary>
        /// Local variables
        /// **********************************************************************
        /// </summary>
        private StreamWriter writer;

        /// <summary>
        /// Constructor to attempt to open the file.  If file exists, open = true;
        /// **********************************************************************
        /// </summary>
        public csvHandleWrite(string fileNameIn)
        {
            fileName = fileNameIn;
            open = false;
            writer = null;

            try
            {
                writer = new StreamWriter(fileName); // (Not thread safe)
                open = true;
            }
            catch (Exception ex_in)
            {
                open = false;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("csvHandleWrite::csvHandleWrite(): The file " + fileName + " could not be written"); System.Diagnostics.Debug.Flush();
#endif
                System.Exception ex_out = new System.Exception("csvHandleWrite::csvHandleWrite(): Couldn't open StreamWriter: " + ex_in.ToString());
                throw ex_out;
            }
        }

        /// <summary>
        /// Destructor (close the file if it isn't already
        /// **********************************************************************
        /// </summary>
        ~csvHandleWrite()
        {
            open = false;
            if(writer != null)
                writer.Close();
        }

        /// <summary>
        /// Close the file if it isn't already
        /// **********************************************************************
        /// </summary>
        public void Close()
        {
            open = false;
            if (writer != null)
                writer.Close();
        }

        /// <summary>
        /// ReadNextToken: Build another csv token and write it to file
        /// **********************************************************************
        /// </summary>
        public bool WriteNextToken(ref List<string> tokenArray)
        {
            if (open == false)
                throw new System.Exception("csvHandleWrite::WriteNextToken(): Trying to write to a closed file");

            // Parse through the input List build up the line to write
            StringBuilder curToken = new StringBuilder();
            List<string>.Enumerator tokenArrayEnum = tokenArray.GetEnumerator();
            
            while(tokenArrayEnum.MoveNext()) // Initially, the enumerator is positioned before the first element in the collection. Returns false if gone to far
            {
                if (curToken.Length == 0)
                    curToken.Append(tokenArrayEnum.Current);
                else
                    curToken.Append("," + tokenArrayEnum.Current);
            }

            // Try writing the next line
            try
            {
                writer.WriteLine(curToken.ToString());
            }
            catch (Exception ex_in)
            {
                open = false;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("csvHandleWrite::WriteNextToken(): The file " + fileName + " could not be written"); System.Diagnostics.Debug.Flush();
#endif
                System.Exception ex_out = new System.Exception("csvHandleWrite::WriteNextToken(): Couldn't write to file: " + ex_in.ToString());
                throw ex_out;
            }

            return true;
        }

        /// <summary>
        /// WriteNextToken: Build another csv token and write it to file (unformatted input)
        /// **********************************************************************
        /// </summary>
        public bool WriteNextToken(string token0, int token1)
        {
            List<string> curToken = new List<string>();

            curToken.Add(token0);
            curToken.Add(String.Format("{0}",token1));

            return this.WriteNextToken(ref curToken);
        }
        public bool WriteNextToken(string token0, string token1)
        {
            List<string> curToken = new List<string>();

            curToken.Add(token0);
            curToken.Add(token1);

            return this.WriteNextToken(ref curToken);
        }
    }

}
