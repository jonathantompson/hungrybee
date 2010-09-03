#region using statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
#endregion

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
        #region Local Variables
        protected string fileName;
        protected bool open;
        #endregion

        #region Default Constructor - csvHandle()
        public csvHandle()
        {
            open = false;
        }
        #endregion

        #region Access and Modifier functions
        // "inline" functions (though not supported in C#)
        public bool IsOpen() { return open; }
        public string GetFileName() { return fileName; }
        #endregion
    }

    /// <summary>
    /// ***********************************************************************
    /// **                          csvHandleRead                            **
    /// ** Reader class to perform fileIO to and from a csv file             **
    /// ***********************************************************************
    /// </summary>
    public class csvHandleRead : csvHandle
    {
        #region Local Variables
        /// Local variables
        /// **********************************************************************
        private StreamReader reader;
        #endregion

        #region Constructor - csvHandleRead(string fileNameIn)
        /// Constructor to attempt to open the file.  If file exists, open = true;
        /// **********************************************************************
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
        #endregion

        #region Default Destructor
        /// Destructor (close the file if it isn't already
        /// **********************************************************************
        ~csvHandleRead()
        {
            if (reader != null)
                reader.Close();
        }
        #endregion

        #region Close()
        /// close the file
        /// **********************************************************************
        public void Close()
        {
            open = false;
            if (reader != null)
                reader.Close();
        }
        #endregion

        #region ReadNextToken()
        /// ReadNextToken: Parse another csv token, return false if we're at the end
        /// **********************************************************************
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
        #endregion
    }

    /// <summary>
    /// ***********************************************************************
    /// **                          csvHandleWrite                           **
    /// ** Reader class to perform fileIO to and from a csv file             **
    /// ***********************************************************************
    /// </summary>
    public class csvHandleWrite : csvHandle
    {
        #region Local Variables
        /// Local variables
        /// **********************************************************************
        private StreamWriter writer;
        #endregion

        #region Constructor - csvHandleWrite(string fileNameIn)
        /// Constructor to attempt to open the file.  If file exists, open = true;
        /// **********************************************************************
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
        #endregion

        #region Default Destructor
        /// Destructor (close the file if it isn't already
        /// **********************************************************************
        ~csvHandleWrite()
        {
            open = false;
            if(writer != null)
                writer.Close();
        }
        #endregion

        #region Close()
        /// Close the file if it isn't already
        /// **********************************************************************
        public void Close()
        {
            open = false;
            if (writer != null)
                writer.Close();
        }
        #endregion

        #region WriteNextToken(ref List<string> tokenArray)
        /// WriteNextToken: Build another csv token and write it to file
        /// **********************************************************************
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
        #endregion

        #region WriteNextToken(string token0, int token1)
        /// WriteNextToken: Build another csv token and write it to file (unformatted input)
        /// **********************************************************************
        public bool WriteNextToken(string token0, int token1)
        {
            List<string> curToken = new List<string>();

            curToken.Add(token0);
            curToken.Add(String.Format("{0}",token1));

            return this.WriteNextToken(ref curToken);
        }
        #endregion

        #region WriteNextToken(string token0, string token1)
        /// WriteNextToken: Build another csv token and write it to file (unformatted input)
        /// **********************************************************************
        public bool WriteNextToken(string token0, string token1)
        {
            List<string> curToken = new List<string>();

            curToken.Add(token0);
            curToken.Add(token1);

            return this.WriteNextToken(ref curToken);
        }
        #endregion
    }

}
