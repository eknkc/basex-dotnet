using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// BaseX provides fast base encoding / decoding of any given alphabet using bitcoin style leading zero compression.
/// It is a .NET port of https://github.com/cryptocoinjs/base-x
/// </summary>
namespace BaseX {
  /// <summary>
  /// Encoding is a custom base encoding defined by an alphabet.
  /// </summary>
  public class Encoding {
    private int charbase;
    private char[] alphabet;
    private Dictionary<char, int> mapper;

    /// <summary>
    /// Creates a new Encoding object that can encode and decode to an arbitrary alphabet
    /// Ordering is important
    /// Sample alphabets:
    ///   - base2: 01
    ///   - base16: 0123456789abcdef
    ///   - base32: 0123456789ABCDEFGHJKMNPQRSTVWXYZ
    ///   - base62: 0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ
    /// </summary>
    /// <param name="alphabetString">Encoding alphabet to be used.</param>
    public Encoding (string alphabetString) {
      if (alphabetString.Length < 2) {
        throw new ArgumentException ("Alphabet should contain at least 2 characters", "alphabetString");
      }

      alphabet = alphabetString.ToCharArray ();
      mapper = new Dictionary<char, int> ();

      for (var i = 0; i < alphabet.Length; i++) {
        var c = alphabet[i];

        if (mapper.ContainsKey (c)) {
          throw new ArgumentException ($"Ambiguous alphabet. character '{c}' repeats", "alphabetString");
        }

        mapper[c] = i;
      }

      charbase = alphabet.Length;
    }

    /// <summary>
    /// Encode function receives a byte array and encodes it to a string using the alphabet provided
    /// </summary>
    /// <param name="input">Input data to be encoded</param>
    /// <returns>Encoded data in the specified alphabet</returns>
    public string Encode (byte[] input) {
      if (input.Length == 0) {
        return "";
      }

      var digits = new List<int> ();

      for (var i = 0; i < input.Length; i++) {
        var carry = (int) input[i];

        for (var j = 0; j < digits.Count; j++) {
          carry += digits[j] << 8;
          digits[j] = carry % charbase;
          carry = carry / charbase;
        }

        while (carry > 0) {
          digits.Add (carry % charbase);
          carry = carry / charbase;
        }
      }

      var res = new StringBuilder ();

      for (var i = 0; i < input.Length - 1 && input[i] == 0; i++) {
        res.Append (alphabet[0]);
      }

      for (var i = digits.Count - 1; i >= 0; i--) {
        res.Append (alphabet[digits[i]]);
      }

      return res.ToString ();
    }

    /// <summary>
    /// Decode function decodes a string previously obtained from Encode, using the same alphabet.
    /// In case the input is not valid an arror will be returned
    /// </summary>
    /// <param name="source">Previously encoded string using the Encode method</param>
    /// <returns>Original data represented by the encoded string</returns>
    public byte[] Decode (string source) {
      if (source.Length == 0) {
        return new byte[] { };
      }

      var bytes = new List<byte> ();

      for (var i = 0; i < source.Length; i++) {
        var c = source[i];
        int index;

        if (!mapper.TryGetValue (c, out index)) {
          throw new ArgumentException ($"Unrecognized character: '{c}'");
        }

        for (var j = 0; j < bytes.Count; j++) {
          index += (int) bytes[j] * charbase;
          bytes[j] = (byte) (index & 0xff);
          index >>= 8;
        }

        while (index > 0) {
          bytes.Add ((byte) (index & 0xff));
          index >>= 8;
        }
      }

      for (var i = 0; i < source.Length - 1 && source[i] == alphabet[0]; i++) {
        bytes.Add (0);
      }

      bytes.Reverse ();

      return bytes.ToArray ();
    }
  }
}
