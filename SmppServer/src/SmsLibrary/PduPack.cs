using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsLibrary
{
    public class PduBitPacker
    {
        #region Fields

        private static byte[] _encodeMask;
        private static byte[] _decodeMask;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the BytePacker class.
        /// </summary>
        static PduBitPacker()
        {
            _encodeMask = new byte[] { 1, 3, 7, 15, 31, 63, 127 };
            _decodeMask = new byte[] { 128, 192, 224, 240, 248, 252, 254 };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Packs an unpacked 7 bit array to an 8 bit packed array according to the GSM
        /// protocol.
        /// </summary>
        /// <param name="unpackedBytes">The byte array that should be packed.</param>
        /// <returns>The packed bytes array.</returns>
        public static byte[] PackBytes(byte[] unpackedBytes)
        {
            return PackBytes(unpackedBytes, false, ' ');
        }

        /// <summary>
        /// Packs an unpacked 7 bit array to an 8 bit packed array according to the GSM
        /// protocol.
        /// </summary>
        /// <param name="unpackedBytes">The byte array that should be packed.</param>
        /// <param name="replaceInvalidChars">Indicates if invalid characters should be replaced by a '?' character.</param>
        /// <returns>The packed bytes array.</returns>
        public static byte[] PackBytes(byte[] unpackedBytes, bool replaceInvalidChars)
        {
            return PackBytes(unpackedBytes, replaceInvalidChars, '?');
        }

        /// <summary>
        /// Packs an unpacked 7 bit array to an 8 bit packed array according to the GSM
        /// protocol.
        /// </summary>
        /// <param name="unpackedBytes">The byte array that should be packed.</param>
        /// <param name="replaceInvalidChars">Indicates if invalid characters should be replaced by the default character.</param>
        /// <param name="defaultChar">The character that replaces invalid characters.</param>
        /// <returns>The packed bytes array.</returns>
        public static byte[] PackBytes(byte[] unpackedBytes, bool replaceInvalidChars, char defaultChar)
        {
            byte defaultByte = (byte)defaultChar;
            byte[] shiftedBytes = new byte[unpackedBytes.Length - (unpackedBytes.Length / 8)];

            int shiftOffset = 0;
            int shiftIndex = 0;

            // Shift the unpacked bytes to the right according to the offset (position of the byte)
            foreach (byte b in unpackedBytes)
            {
                byte tmpByte = b;

                // Handle invalid characters (bytes out of range)
                if (tmpByte > 127)
                {
                    if (!replaceInvalidChars)
                    {
                        // throw exception and exit the method
                        throw new Exception("Invalid character detected: " + tmpByte.ToString("X2"));
                    }
                    else
                    {
                        tmpByte = defaultByte;
                    }
                }

                // Perform the byte shifting
                if (shiftOffset == 7)
                {
                    shiftOffset = 0;
                }
                else
                {
                    shiftedBytes[shiftIndex] = (byte)(tmpByte >> shiftOffset);
                    shiftOffset++;
                    shiftIndex++;
                }
            }

            int moveOffset = 1;
            int moveIndex = 1;
            int packIndex = 0;
            byte[] packedBytes = new byte[shiftedBytes.Length];

            // Move the bits to the appropriate byte (pack the bits)
            foreach (byte b in unpackedBytes)
            {
                if (moveOffset == 8)
                {
                    moveOffset = 1;
                }
                else
                {
                    if (moveIndex != unpackedBytes.Length)
                    {
                        // Extract the bits to be moved
                        int extractedBitsByte = (unpackedBytes[moveIndex] & _encodeMask[moveOffset - 1]);
                        // Shift the extracted bits to the proper offset
                        extractedBitsByte = (extractedBitsByte << (8 - moveOffset));
                        // Move the bits to the appropriate byte (pack the bits)
                        int movedBitsByte = (extractedBitsByte | shiftedBytes[packIndex]);

                        packedBytes[packIndex] = (byte)movedBitsByte;

                        moveOffset++;
                        packIndex++;
                    }
                    else
                    {
                        packedBytes[packIndex] = shiftedBytes[packIndex];
                    }
                }

                moveIndex++;
            }

            return packedBytes;
        }


        /// <summary>
        ///  Unpacks a packed 8 bit array to a 7 bit unpacked array according to the GSM
        ///  Protocol.
        /// </summary>
        /// <param name="packedBytes">The byte array that should be unpacked.</param>
        /// <returns>The unpacked bytes array.</returns>
        public static byte[] UnpackBytes(byte[] packedBytes)
        {
            byte[] shiftedBytes = new byte[(packedBytes.Length * 8) / 7];

            int shiftOffset = 0;
            int shiftIndex = 0;

            // Shift the packed bytes to the left according to the offset (position of the byte)
            foreach (byte b in packedBytes)
            {
                if (shiftOffset == 7)
                {
                    shiftedBytes[shiftIndex] = 0;
                    shiftOffset = 0;
                    shiftIndex++;
                }

                shiftedBytes[shiftIndex] = (byte)((b << shiftOffset) & 127);

                shiftOffset++;
                shiftIndex++;
            }

            int moveOffset = 0;
            int moveIndex = 0;
            int unpackIndex = 1;
            byte[] unpackedBytes = new byte[shiftedBytes.Length];

            // 
            if (shiftedBytes.Length > 0)
            {
                unpackedBytes[unpackIndex - 1] = shiftedBytes[unpackIndex - 1];
            }

            // Move the bits to the appropriate byte (unpack the bits)
            foreach (byte b in packedBytes)
            {
                if (unpackIndex != shiftedBytes.Length)
                {
                    if (moveOffset == 7)
                    {
                        moveOffset = 0;
                        unpackIndex++;
                        unpackedBytes[unpackIndex - 1] = shiftedBytes[unpackIndex - 1];
                    }

                    if (unpackIndex != shiftedBytes.Length)
                    {
                        // Extract the bits to be moved
                        int extractedBitsByte = (packedBytes[moveIndex] & _decodeMask[moveOffset]);
                        // Shift the extracted bits to the proper offset
                        extractedBitsByte = (extractedBitsByte >> (7 - moveOffset));
                        // Move the bits to the appropriate byte (unpack the bits)
                        int movedBitsByte = (extractedBitsByte | shiftedBytes[unpackIndex]);

                        unpackedBytes[unpackIndex] = (byte)movedBitsByte;

                        moveOffset++;
                        unpackIndex++;
                        moveIndex++;
                    }
                }
            }

            // Remove the padding if exists
            if (unpackedBytes[unpackedBytes.Length - 1] == 0)
            {
                byte[] finalResultBytes = new byte[unpackedBytes.Length - 1];
                Array.Copy(unpackedBytes, 0, finalResultBytes, 0, finalResultBytes.Length);

                return finalResultBytes;
            }

            return unpackedBytes;
        }


        /// <summary>
        /// Converts hex string into the equivalent byte array.
        /// </summary>
        /// <param name="hexString">The hex string to be converted.</param>
        /// <returns>The equivalent byte array.</returns>
        public static byte[] ConvertHexToBytes(string hexString)
        {
            if (hexString.Length % 2 != 0)
                return null;

            int len = hexString.Length / 2;
            byte[] array = new byte[len];

            for (int i = 0; i < array.Length; i++)
            {
                string tmp = hexString.Substring(i * 2, 2);
                array[i] = byte.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
            }

            return array;
        }

        /// <summary>
        /// Converts a byte array into the equivalent hex string.
        /// </summary>
        /// <param name="byteArray">The byte array to be converted.</param>
        /// <returns>The equivalent hex string.</returns>
        public static string ConvertBytesToHex(byte[] byteArray)
        {
            if (byteArray == null)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteArray)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        #endregion
    }
}
