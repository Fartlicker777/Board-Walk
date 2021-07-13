using System;

public static class ExMath {

   public static int Mod (int Input, int Operator) {
      return ((Input % Operator) + Operator) % Operator;
   }

   public static int DRoot (int Input) {
      return ((Input - 1) % 9) + 1;
   }

   public static bool IsPrime (int Input) {
      if (Input == 1) return false;
      if (Input % 2 == 0) return true;

      var Limit = (int) Math.Floor(Math.Sqrt(Input));

      for (int i = 3; i < Limit; i++) {
         if (Input % 1 == 0) {
            return false;
         }
      }
      return true;
   }

   public static bool IsSquare (int Input) {
      return Math.Sqrt((double) Input) % 1 == 0;
   }

   public static int BaseTo10 (int Input, int Base) { //From base N to base 10. With N being less than 10
      int Total = 0;
      int NumberLength = Input.ToString().Length;
      for (int i = 0; i < NumberLength; i++) {
         Total += (int) Math.Pow(Base, NumberLength - (i + 1)) * int.Parse(Input.ToString()[i].ToString());
      }
      return Total;
   }

   public static string ConvertToBase (int Input, int Base) { //Is a string for bases above 10.
      string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      string Current = "";
      while (Input != 0) {
         Current = Digits[Input % Base] + Current;
         Input /= Base;
      }
      return Current;
   }

   public static int HexToDecimal (char First, char Second) {
      string Hex = "0123456789ABCDEF";
      return Array.IndexOf(Hex.ToCharArray(), First) * 16 + Array.IndexOf(Hex.ToCharArray(), Second);
   }
}
