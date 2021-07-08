using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class BoardWalk : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public GameObject[] LeftDie;
   public GameObject[] RightDie;

   public SpriteRenderer TokenSR;
   public Sprite[] TokenS;

   public KMSelectable Switch;
   public GameObject[] UpDownSwitch;

   public KMSelectable Screen;
   public TextMesh CardText;

   public TextMesh[] SubmissionTexts;
   public KMSelectable SubmitButton;

   public TextMesh StageIndicator;
   public KMSelectable StageRecoveryButton;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   //                 0    1  2    3    4   5   6  7    8  9  10  11  12  13 14  15  16  17  18  19   20  21  22   23  24   25  26  27  28   29  30   31  32  33   34   35   36  37  38  39
   int[] TheBoard = { 100, 0, 101, 1, 102, 103, 2, 104, 3, 4, 105, 5, 106, 6, 7, 107, 8, 101, 9, 10, 108, 11, 104, 12, 13, 109, 14, 15, 110, 16, 111, 17, 18, 101, 19, 112, 104, 20, 113, 21 };
   List<int[]> DieRolls = new List<int[]> { };
   int[] InitialCards = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
   int[] InitialCardsChest = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
   int[] ColorVisitations = new int[8];
   int[] RailroadVisitations = new int[4]; //Is an int array instead bool so that I can do .Sum and use multiplying bullshit
   int Token;
   int Mods;
   int IgnoredMods;
   int Stage = -1;
   int Jailed;
   int CChestProg = -1;
   int ChanceProg = -1;
   int CChestIndex;
   int ChanceIndex;
   int InputSubmission;
   int StrikesGained;
   int StageRecoveryActivations;
   int CurrentPosition;
   int ConsecuativeDoubles;
   int Debt;

   float[] CChestTextSizes = {
      0.0009777471f,
      0.0007653747f,
      0.001036426f,
      0.0007509612f,
      0.0005651942f,
      0.0008035284f,
      0.0008035284f,
      0.0008035284f,
      0.0008035284f,
      0.00106463f,
      0.00106463f,
      0.0008002994f,
      0.0006099612f,
      0.001218359f };
   float[] ChanceTextSizes = {
      0.0009777471f,
      0.0009777471f,
      0.0009777471f,
      0.0007879253f,
      0.0005612394f,
      0.0007269522f,
      0.0008207211f,
      0.001191174f,
      0.0005651942f,
      0.0009585879f,
      0.0007790008f,
      0.0005676657f,
      0.0007653747f};

   List<bool> JailedTurns = new List<bool> { };

   public static string[] ignoredModules = null;
   string[] ChanceQuotes = {
      "Advance to\nBoardwalk",
      "Advance to\nGo",
      "Advance to\nIllinois Avenue",
      "Advance to\nSt. Charles\nPlace",
      "Advance to the nearest\nrailroad and pay\ntwice the amount",
      "Advance to\nthe nearest\nutility",
      "Bank pays\nyou dividend\nof $50",
      "Go back 3\nspaces",
      "Go to jail. Go directly\nto jail, do not pass Go,\ndo not collect $200",
      "Speeding fine\n$15",
      "Take a trip\nto Reading\nRailroad",
      "You have been\nelected Discord\nAdmin. Pay each\nplayer $50",
      "Your building\nloan matures.\nCollect $150"
   };
   string[] CChestQuotes = {
      "Advance to\nGo",
      "Bank error in\nyour favor.\nCollect $200",
      "Doctor's fees.\nPay $200",
      "From sale of\nstock you\nget $200",
      "Go to jail. Go directly\nto jail, do not pass Go,\ndo not collect $200",
      "Holiday fund\nmatures.\nReceive $100",
      "Income Tax\nrefund. Collect\n$20",
      "It is your\nbirthday.\nCollect $10",
      "Life Insurance\nmatters. Collect\n$100",
      "Pay hospital\nfees of $100",
      "Pay school\nfees of $50",
      "Receive $25\nconsultancy\nfee",
      "You have won second\nplace in a beauty\ncontest. Collect $10",
      "You inherit\n$100"
   };
   string FinalAnswer = "";

   bool HasRan;
   bool IgnoreRan;
   bool Solvable;
   bool Animating;
   bool UpdateStop;
   bool NeedToJail;
   bool ElectricVisited;
   bool WaterVisited;

   void Awake () {
      moduleId = moduleIdCounter++;

      Switch.OnInteract += delegate () { SwitchPress(); return false; };
      Screen.OnInteract += delegate () { ScreenPress(); return false; };
      SubmitButton.OnInteract += delegate () { SubmitButtonPress(); return false; };
      StageRecoveryButton.OnInteract += delegate () { StageRecoveryPress(); return false; };

      if (ignoredModules == null) {
         ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("The Board Walk", new string[] {
                "14",
                "42",
                "501",
                "A>N<D",
                "Bamboozling Time Keeper",
                "Black Arrows",
                "Brainf---",
                "The Board Walk",
                "Busy Beaver",
                "Don't Touch Anything",
                "Floor Lights",
                "Forget Any Color",
                "Forget Enigma",
                "Forget Ligma",
                "Forget Everything",
                "Forget Infinity",
                "Forget It Not",
                "Forget Maze Not",
                "Forget Me Later",
                "Forget Me Not",
                "Forget Perspective",
                "Forget The Colors",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Iconic",
                "Keypad Directionality",
                "Kugelblitz",
                "Multitask",
                "OmegaDestroyer",
                "OmegaForest",
                "Organization",
                "Password Destroyer",
                "Purgatory",
                "RPS Judging",
                "Security Council",
                "Shoddy Chess",
                "Simon Forgets",
                "Simon's Stages",
                "Souvenir",
                "Speech Jammer",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn The Key",
                "The Twin",
                "Übermodule",
                "Ultimate Custom Night",
                "The Very Annoying Button",
                "Whiteout"
            });
      }
   }

   #region Button Presses

   void StageRecoveryPress () {
      Audio.PlaySoundAtTransform("Bonk", transform);
      Screen.AddInteractionPunch();
      if (StrikesGained > StageRecoveryActivations) {
         StartCoroutine(StageRecovery());
         StageRecoveryActivations++;
      }
   }

   void SubmitButtonPress () {
      Audio.PlaySoundAtTransform("Bonk", transform);
      Screen.AddInteractionPunch();
      if (moduleSolved) {
         return;
      }
      if (!Solvable) {
         if (!JailedTurns[Stage]) {
            GetComponent<KMBombModule>().HandleStrike();
         }
         NeedToJail = false;
      }
      else {
         StartCoroutine(CheckAnimation());
      }
   }

   void ScreenPress () {
      Audio.PlaySoundAtTransform("Bonk", transform);
      Screen.AddInteractionPunch();
      if (Animating || moduleSolved) {
         return;
      }
      if (Solvable) {
         InputSubmission++;
         InputSubmission %= 10;
         SubmissionTexts[5].text = InputSubmission.ToString();
      }
      else if (UpDownSwitch[0].activeSelf) {
         ChanceIndex++;
         ChanceIndex %= ChanceQuotes.Length;
         StartCoroutine(DisplayText(ChanceQuotes[InitialCards[ChanceIndex]], true));
      }
      else {
         CChestIndex++;
         CChestIndex %= CChestQuotes.Length;
         StartCoroutine(DisplayText(CChestQuotes[InitialCardsChest[CChestIndex]], false));
      }
   }

   void SwitchPress () {
      if (Animating) {
         return;
      }
      Audio.PlaySoundAtTransform("flip", transform);
      UpDownSwitch[0].SetActive(!UpDownSwitch[0].activeSelf);
      UpDownSwitch[1].SetActive(!UpDownSwitch[1].activeSelf);
      if (moduleSolved) {
         return;
      }
      if (!Solvable) {
         if (UpDownSwitch[0].activeSelf) {
            StartCoroutine(DisplayText(ChanceQuotes[InitialCards[ChanceIndex]], true));
         }
         else {
            StartCoroutine(DisplayText(CChestQuotes[InitialCardsChest[CChestIndex]], false));
         }
      }
      else {
         for (int i = 1; i < 5; i++) {
            SubmissionTexts[i].text = SubmissionTexts[i + 1].text;
         }
         InputSubmission = 0;
         SubmissionTexts[5].text = "0";
      }
   }

   #endregion

   #region Animations

   IEnumerator SolveAnimation () {
      Audio.PlaySoundAtTransform("Solve", transform);
      string[] Colors = {
         "783f04",
         "76a5af",
         "ff00ff",
         "ff9900",
         "ff0000",
         "ffff00",
         "33ac01",
         "1155cc"
      };
      while (true) {
         for (int j = 0; j < 8; j++) {
            for (int i = 0; i < 6; i++) {
               SubmissionTexts[i].color = new Color32((byte) HexToDecimal(Colors[j][0], Colors[j][1]), (byte) HexToDecimal(Colors[j][2], Colors[j][3]), (byte) HexToDecimal(Colors[j][4], Colors[j][5]), 255);
               yield return new WaitForSeconds(.1f);
            }
         }
      }
   }

   IEnumerator StageRecovery () {
      Animating = true;
      for (int i = 0; i < 6; i++) {
         LeftDie[i].SetActive(false);
         RightDie[i].SetActive(false);
      }
      for (int i = 0; i < DieRolls.Count(); i++) {
         if (i != 0) {
            LeftDie[DieRolls[i - 1][0] - 1].SetActive(false);
            RightDie[DieRolls[i - 1][1] - 1].SetActive(false);
         }
         LeftDie[DieRolls[i][0] - 1].SetActive(true);
         RightDie[DieRolls[i][1] - 1].SetActive(true);
         StageIndicator.text = (i + 1).ToString("000");
         yield return new WaitForSeconds(1f);
      }
      for (int i = 0; i < 6; i++) {
         SubmissionTexts[i].text = "";
      }
      StageIndicator.text = "";
      ChanceIndex = 0;
      CChestIndex = 0;
      if (UpDownSwitch[0].activeSelf) {
         for (int i = 0; i < 13; i++) {
            StartCoroutine(DisplayText(ChanceQuotes[InitialCards[ChanceIndex]], true));
            while (Animating) {
               yield return null;
            }
            ChanceIndex++;
            ChanceIndex %= ChanceQuotes.Length;
            yield return new WaitForSeconds(1f);
         }
      }
      else {
         for (int i = 0; i < 14; i++) {
            DisplayText(CChestQuotes[InitialCardsChest[i]], false);
            while (Animating) {
               yield return null;
            }
            CChestIndex++;
            CChestIndex %= CChestQuotes.Length;
            yield return new WaitForSeconds(1f);
         }
      }
      CardText.text = "";
      SubmissionTexts[0].text = "$";
      for (int i = 1; i < 6; i++) {
         SubmissionTexts[i].text = "0";
      }
      Animating = false;
   }

   IEnumerator DisplayText (string Input, bool Up) {
      CardText.text = "";
      if (Up) {
         if (ChanceIndex == 0) {
            CardText.color = new Color32(0, 255, 0, 255);
         }
         else {
            CardText.color = new Color32(255, 255, 255, 255);
         }
         CardText.transform.localScale = new Vector3(ChanceTextSizes[InitialCards[ChanceIndex]], ChanceTextSizes[InitialCards[ChanceIndex]], ChanceTextSizes[InitialCards[ChanceIndex]]);
      }
      else {
         if (CChestIndex == 0) {
            CardText.color = new Color32(0, 255, 0, 255);
         }
         else {
            CardText.color = new Color32(255, 255, 255, 255);
         }
         CardText.transform.localScale = new Vector3(CChestTextSizes[InitialCardsChest[CChestIndex]], CChestTextSizes[InitialCardsChest[CChestIndex]], CChestTextSizes[InitialCardsChest[CChestIndex]]);
      }
      Animating = true;
      for (int i = 0; i < Input.Length; i++) {
         CardText.text += Input[i].ToString();
         if (Input[i] != '\n') {
            yield return new WaitForSeconds(.01f);
         }
      }
      Animating = false;
   }

   IEnumerator CheckAnimation () {
      bool WillStrike = false;
      for (int i = 0; i < 6; i++) {
         if (SubmissionTexts[i].text == FinalAnswer[i].ToString() && !WillStrike) {
            SubmissionTexts[i].color = new Color32(0, 255, 0, 255);
         }
         else {
            WillStrike = true;
            SubmissionTexts[i].color = new Color32(255, 0, 0, 255);
         }
         yield return new WaitForSeconds(.1f);
      }
      if (WillStrike) {
         GetComponent<KMBombModule>().HandleStrike();
         StrikesGained++;
         for (int i = 0; i < 6; i++) {
            SubmissionTexts[i].color = new Color32(255, 255, 255, 255);
            yield return new WaitForSeconds(.1f);
         }
      }
      else {
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
         StartCoroutine(SolveAnimation());
      }
   }

   #endregion

   int HexToDecimal (char First, char Second) {
      string Hex = "0123456789ABCDEF";
      return Array.IndexOf(Hex.ToCharArray(), First) * 16 + Array.IndexOf(Hex.ToCharArray(), Second);
   }

   void Start () {
      InitialCards.Shuffle();
      InitialCardsChest.Shuffle();
      CardText.text = ChanceQuotes[InitialCards[0]];
      CardText.transform.localScale = new Vector3(ChanceTextSizes[InitialCards[ChanceIndex]], ChanceTextSizes[InitialCards[ChanceIndex]], ChanceTextSizes[InitialCards[ChanceIndex]]);
      CardText.color = new Color32(0, 255, 0, 255);
      UpDownSwitch[1].SetActive(false);
      for (int i = 0; i < 6; i++) {
         LeftDie[i].SetActive(false);
         RightDie[i].SetActive(false);
      }
      Token = Rnd.Range(0, 8);
      TokenSR.sprite = TokenS[Token];
      switch (Token) {
         case 0:
            TokenSR.transform.localScale = new Vector3(.002f, .002f, .002f);                       //thimble
            break;
         case 1:
            TokenSR.transform.localScale = new Vector3(0.005074251f, 0.005074251f, 0.005074251f);  //Wheel
            break;
         case 2:
            TokenSR.transform.localScale = new Vector3(0.01237729f, 0.01237729f, 0.01237729f);     //cat
            break;
         case 3:
            TokenSR.transform.localScale = new Vector3(0.005531019f, 0.005531019f, 0.005531019f);  //dog
            break;
         case 4:
            TokenSR.transform.localScale = new Vector3(0.005778752f, 0.005778752f, 0.005778752f);  //car
            break;
         case 5:
            TokenSR.transform.localScale = new Vector3(0.004434305f, 0.004434305f, 0.004434305f);  //iron
            break;
         case 6:
            TokenSR.transform.localScale = new Vector3(0.004851759f, 0.004851759f, 0.004851759f);  //hat
            break;
         case 7:
            TokenSR.transform.localScale = new Vector3(0.01343577f, 0.01343577f, 0.01343577f);     //ship
            break;
      }

      Debug.LogFormat("[The Board Walk #{0}] Your token is the {1}.", moduleId, new string[8] { "Thimble", "Wheelbarrow", "Cat", "Dog", "Car", "Iron", "Top Hat", "Battleship" }[Token]);
   }

   #region Board Calculations

   void Update () {
      if (UpdateStop) {
         return;
      }
      if (!IgnoreRan && ignoredModules != null) {
         for (int i = 0; i < Bomb.GetSolvableModuleNames().Count(); i++) {
            for (int j = 0; j < ignoredModules.Length; j++) {
               if (Bomb.GetSolvableModuleNames()[i] == ignoredModules[j]) {
                  IgnoredMods++;
               }
            }
         }
         IgnoreRan = true;
      }
      if (Mods == 0 && !HasRan) {
         Mods = Bomb.GetSolvableModuleNames().Count() - IgnoredMods;
         if (Bomb.GetSolvableModuleNames().Count() == IgnoredMods) {
            StartCoroutine(SolveAnimation());
         }
         if (Mods != 0) {
            for (int i = 0; i < Mods; i++) {
               DieRolls.Add(new int[] { Rnd.Range(1, 7), Rnd.Range(1, 7) });
               Debug.LogFormat("[The Board Walk #{0}] The roll at stage {1} is {2}.", moduleId, i + 1, DieRolls[i].Join(" & "));
               if (Jailed == 0) {
                  BoardMovement(DieRolls[i][0], DieRolls[i][1]);
               }
               else {
                  Debug.LogFormat("[The Board Walk #{0}] You have {1} more turns in Jail.", moduleId, Jailed);
                  if (Jailed == 3) {
                     JailedTurns[i - 1] = true;
                  }
                  Jailed--;
               }
               JailedTurns.Add(false);
            }
            Debt = Debt < 0 ? 0 : Debt % 100000;
            FinalAnswer = "$" + Debt.ToString("00000");
            Debug.LogFormat("[The Board Walk #{0}] The final amount you have to pay is {1}.", moduleId, FinalAnswer);
            HasRan = true;
         }
      }
      int Solved = Bomb.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x));
      if (Solved == Mods) {
         Solvable = true;
         CardText.text = "";
         StageIndicator.text = "";
         SubmissionTexts[0].text = "$";
         for (int i = 1; i < 6; i++) {
            SubmissionTexts[i].text = "0";
         }
         UpdateStop = true;
      }
      else if (Solved > Stage) {
         if (NeedToJail) {
            GetComponent<KMBombModule>().HandleStrike();
            NeedToJail = false;
         }
         Stage++;
         StageIndicator.text = ((Stage + 1) % 1000).ToString("000");
         if (JailedTurns[Stage]) {
            NeedToJail = true;
         }
         if (Stage != 0) {
            LeftDie[DieRolls[Stage - 1][0] - 1].SetActive(false);
            RightDie[DieRolls[Stage - 1][1] - 1].SetActive(false);
         }
         LeftDie[DieRolls[Stage][0] - 1].SetActive(true);
         RightDie[DieRolls[Stage][1] - 1].SetActive(true);
      }
   }

   void BoardMovement (int Left, int Right) {
      if (Left == Right) {
         ConsecuativeDoubles++;
      }
      else {
         ConsecuativeDoubles = 0;
      }
      if (ConsecuativeDoubles == 3) { //Leave this for later
         ConsecuativeDoubles %= 3;
         Jailed = 3;
         Debug.LogFormat("[The Board Walk #{0}] Uh oh. You rolled three doubles.", moduleId);
         Debt += Jail();
         return;
      }
      CurrentPosition += Left + Right;
      if (Token == 3) {
         CurrentPosition++;
      }
      if (CurrentPosition >= 40) {
         Debt += Go();
         CurrentPosition %= 40;
      }
      switch (TheBoard[CurrentPosition] / 100) {
         case 0:
            Debt += PropertyDebtCollector();
            break;
         case 1:
            Debt += NonPropertyCollector();
            break;
      }
      CallDebt();
   }

   int PropertyDebtCollector () {
      if (!Places[TheBoard[CurrentPosition]].Visited) {
         Places[TheBoard[CurrentPosition]].Visited = true;
         ColorVisitations[Places[TheBoard[CurrentPosition]].ColorID]++;
      }
      switch (Places[TheBoard[CurrentPosition]].ColorID) {
         case 0:
         case 7:
            if (ColorVisitations[Places[TheBoard[CurrentPosition]].ColorID] == 2) {
               Places[TheBoard[CurrentPosition]].DoubleCash = true;
            }
            break;
         default:
            if (ColorVisitations[Places[TheBoard[CurrentPosition]].ColorID] == 3) {
               Places[TheBoard[CurrentPosition]].DoubleCash = true;
            }
            break;
      }
      int Temp = Places[TheBoard[CurrentPosition]].Values[Places[TheBoard[CurrentPosition]].Progression];
      if (Places[TheBoard[CurrentPosition]].DoubleCash) {
         Temp *= 2;
      }
      if (Places[TheBoard[CurrentPosition]].Progression != 5) {
         Places[TheBoard[CurrentPosition]].Progression++;
      }

      Debug.LogFormat("[The Board Walk #{0}] You have landed on {1}. You have to pay ${2}.", moduleId, Places[TheBoard[CurrentPosition]].Name, Temp);
      return Temp;
   }

   /* Board Meanings
100 = Go
101 = Community Chest
102 = Income Tax
103 = Reading
104 = Chance
105 = Jail
106 = Electric
107 = Pennsylvania Railroad
108 = Free Parking
109 = BO
110 = Water
111 = Go to Jail
112 = Short
113 = Luxury
*/

   int NonPropertyCollector () {
      switch (TheBoard[CurrentPosition] % 100) {
         case 0:
            return Go();
         case 1:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Community Chest.", moduleId);
            return CommunityChest();
         case 2:
            if (Token == 6) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Income Tax, but you don't pay anything.", moduleId);
               return 0;
            }
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Income Tax. You have to pay $200.", moduleId);
            return 200;
         case 3:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Reading Railroad. You have to pay ${1}.", moduleId, Railroad(0));
            return Railroad(0);
         case 4:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Chance.", moduleId);
            return Chance();
         case 5:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Just Visiting.", moduleId);
            if (Token == 0) { return -100; }
            else return 0;
         case 6:
            ElectricVisited = true;
            if (Token == 5) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Electrical Company free of charge.", moduleId);
               return 0;
            }
            if (WaterVisited) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Electrical Company. You have to pay ${1}.", moduleId, 10 * (DieRolls.Last()[0] + DieRolls.Last()[1]));
               return 10 * (DieRolls.Last()[0] + DieRolls.Last()[1]);
            }
            else {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Electrical Company. You have to pay ${1}.", moduleId, 4 * (DieRolls.Last()[0] + DieRolls.Last()[1]));
               return 4 * (DieRolls.Last()[0] + DieRolls.Last()[1]);
            }
         case 7:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Pennsylvania Railroad. You have to pay ${1}.", moduleId, Railroad(1));
            return Railroad(1);
         case 8:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Free Parking.", moduleId);
            if (Token == 0) {
               Debug.LogFormat("[The Board Walk #{0}] You gain $100.", moduleId);
               return -100;
            }
            else return 0;
         case 9:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on B.O. Railroad. You have to pay ${1}.", moduleId, Railroad(2));
            return Railroad(2);
         case 10:
            WaterVisited = true;
            if (Token == 5) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Electrical Company free of charge.", moduleId);
               return 0;
            }
            if (ElectricVisited) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Water Works. You have to pay ${1}.", moduleId, 10 * (DieRolls.Last()[0] + DieRolls.Last()[1]));
               return 10 * (DieRolls.Last()[0] + DieRolls.Last()[1]);
            }
            else {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Water Works. You have to pay ${1}.", moduleId, 4 * (DieRolls.Last()[0] + DieRolls.Last()[1]));
               return 4 * (DieRolls.Last()[0] + DieRolls.Last()[1]);
            }
         case 11:
            return Jail();
         case 12:
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Short Line. You have to pay ${1}.", moduleId, Railroad(3));
            return Railroad(3);
         case 13:
            if (Token == 6) {
               Debug.LogFormat("[The Board Walk #{0}] You have landed on Luxury Tax, but you don't pay anything.", moduleId);
               return 0;
            }
            Debug.LogFormat("[The Board Walk #{0}] You have landed on Luxury Tax. You have to pay $100.", moduleId);
            return 100;
         default:
            return 0;
      }
   }

   /* Community
 * Go
 * Bank error +200
 * Doctor -50
 * Stock +50
 * Go to jail and die
 * Holiday fund, +100
 * Income tax refund, +20
 * Birthday +10
 * Life insurance +100
 * Hospital -100
 * School -50
 * Consultancy +25
 * Beauty +10
 * Inherit +100
 */

   int CommunityChest () { //CChest and Chance need to be ints because they break for no fucking reason.
      //Debug.Log(InitialCardsChest[CChestProg]);
      CChestProg = (CChestProg + 1) % 14;
      switch (InitialCardsChest[CChestProg]) {
         case 0:
            Debug.LogFormat("[The Board Walk #{0}] Advance directly to Go.", moduleId);
            return Go();
         case 1:
            Debug.LogFormat("[The Board Walk #{0}] Bank error in your favor, collect $200.", moduleId, Debt);
            return -200;
         case 2:
            Debug.LogFormat("[The Board Walk #{0}] Doctor's fees. Pay $50.", moduleId, Debt);
            return 50;
         case 3:
            Debug.LogFormat("[The Board Walk #{0}] From sale of stock, you get $200.", moduleId, Debt);
            return -200;
         case 4:
            return Jail();
         case 5:
            Debug.LogFormat("[The Board Walk #{0}] Holiday fund matures, collect $100.", moduleId, Debt);
            return -100;
         case 6:
            Debug.LogFormat("[The Board Walk #{0}] Income tax refund, collect $20.", moduleId, Debt);
            return -20;
         case 7:
            Debug.LogFormat("[The Board Walk #{0}] It is your birthday. Collect $10 from every player.", moduleId, Debt);
            return -10;
         case 8:
            Debug.LogFormat("[The Board Walk #{0}] Life insurance matters. Collect $100.", moduleId, Debt);
            return -100;
         case 9:
            Debug.LogFormat("[The Board Walk #{0}] Pay hospital fees of $100.", moduleId, Debt);
            return 100;
         case 10:
            Debug.LogFormat("[The Board Walk #{0}] Pay school fees of $50.", moduleId, Debt);
            return 50;
         case 11:
            Debug.LogFormat("[The Board Walk #{0}] Receive $25 consultancy fee.", moduleId, Debt);
            return -25;
         case 12:
            Debug.LogFormat("[The Board Walk #{0}] You have won second prize in a beauty contest. Collect $10.", moduleId, Debt);
            return -10;
         case 13:
            Debug.LogFormat("[The Board Walk #{0}] You inherit $100.", moduleId, Debt);
            return -100;
         default:
            return 0;
      }

   }

   int Go () {
      Debug.LogFormat("[The Board Walk #{0}] You have landed on Go. Collect $200.", moduleId);
      if (Token == 1) {
         Debug.LogFormat("[The Board Walk #{0}] Receive an extra $50 for being a wheelbarrow.", moduleId);
         return -250;
      }
      return -200;
   }

   int Jail () {
      CurrentPosition = 10;
      Debug.LogFormat("[The Board Walk #{0}] Go to Jail. Go directly to Jail, do not pass Go, do not collect $200.", moduleId);
      if (Token == 4) {
         return 100;
      }
      else {
         return 0;
      }
   }

   /* Chance
 * Advance to Boardwalk
 * Advance to Go
 * Advance to Illinois
 * Advance to St. Charles
 * Advance to the nearest Railroad
 * Advance to the nearest Utility
 * Bank pays you $50
 * Go back 3 spaces
 * Go directly to Jail
 * Speeding fine of $15
 * Go to Reading Railroad
 * Pay $50        (chairman)
 * Collect $150   (Building loan)
 */

   int Chance () {
      ChanceProg = (ChanceProg + 1) % 13;
      switch (InitialCards[ChanceProg]) {
         case 0:
            AdvanceUntilSomewhere(39);
            Debug.LogFormat("[The Board Walk #{0}] Advance directly to Boardwalk.", moduleId);
            return PropertyDebtCollector();
         case 1:
            Debug.LogFormat("[The Board Walk #{0}] Advance directly to Go.", moduleId);
            return Go();
         case 2:
            Debug.LogFormat("[The Board Walk #{0}] Advance directly to Illinois Avenue.", moduleId);
            AdvanceUntilSomewhere(24);
            return PropertyDebtCollector();
         case 3:
            Debug.LogFormat("[The Board Walk #{0}] Advance directly to St. Charles.", moduleId);
            AdvanceUntilSomewhere(11);
            return PropertyDebtCollector();
         case 4:
            bool Temp = true;
            while (Temp) {
               CurrentPosition++;
               if (TheBoard[CurrentPosition] / 100 == 1) {
                  if (TheBoard[CurrentPosition] % 100 == 3 || TheBoard[CurrentPosition] % 100 == 7 || TheBoard[CurrentPosition] % 100 == 9 || TheBoard[CurrentPosition] % 100 == 12) {
                     Debug.LogFormat("[The Board Walk #{0}] Advance to the nearest railroad. Pay twice the rental to which they are otherwise entitled.", moduleId);
                     Temp = false;
                  }
               }
            }
            return 2 * NonPropertyCollector();
         case 5:
            if (CurrentPosition >= 12 && CurrentPosition < 28) {
               AdvanceUntilSomewhere(28);
            }
            else {
               AdvanceUntilSomewhere(12);
            }
            Debug.LogFormat("[The Board Walk #{0}] Advance to the nearest Utility.", moduleId);
            return NonPropertyCollector();
         case 6:
            Debug.LogFormat("[The Board Walk #{0}] Bank pays you $50.", moduleId, Debt);
            return -50;
         case 7:
            CurrentPosition -= 3;
            CurrentPosition = ExMath.Mod(CurrentPosition, 40);
            Debug.LogFormat("[The Board Walk #{0}] Go back 3 spaces.", moduleId);
            switch (TheBoard[CurrentPosition] / 100) {
               case 0:
                  return PropertyDebtCollector();
               case 1:
                  return NonPropertyCollector();
               default:
                  return 0;
            }
         case 8:
            return Jail();
         case 9:
            Debug.LogFormat("[The Board Walk #{0}] Speeding fine of $15.", moduleId);
            return 15;
         case 10:
            Debug.LogFormat("[The Board Walk #{0}] Advance to Reading Railroad.", moduleId);
            CurrentPosition = 5;
            return Railroad(0) + Go();
         case 11:
            Debug.LogFormat("[The Board Walk #{0}] You have been elected Discord Admin. Pay $50 to everyone.", moduleId);
            return 50;
         case 12:
            Debug.LogFormat("[The Board Walk #{0}] Your building loan matures. Receive $150.", moduleId, Debt);
            return -150;
         default:
            return 0;
      }
   }

   int Railroad (int Input) {
      if (RailroadVisitations[Input] == 0) {
         RailroadVisitations[Input]++;
      }
      //Debug.Log(RailroadVisitations.Sum());
      if (Token == 2) { return 25; }
      //Debug.Log((int) (((double) 25 / 2) * (int) Math.Pow(2, RailroadVisitations.Sum())));
      return (int) (((double) 25 / 2) * (int) Math.Pow(2, RailroadVisitations.Sum()));
   }

   void CallDebt () {
      Debug.LogFormat("[The Board Walk #{0}] Your debt is now ${1}.", moduleId, Debt);
   }

   void AdvanceUntilSomewhere (int Target) {
      while (CurrentPosition != Target) {
         CurrentPosition++;
         CurrentPosition %= 40;
         if (CurrentPosition == 0) {
            Debt += Go();
         }
      }
      //Debug.Log(CurrentPosition);
   }

   #endregion

   #region Property Declaration

   class Properties {
      public string Name { get; set; }
      public int ColorID { get; set; }
      public int Progression { get; set; }
      //int ColorAmount { get; set; }
      public bool Visited { get; set; }
      public bool DoubleCash { get; set; }
      public int[] Values { get; set; }

      public Properties (string Name, int ColorID, /*int Color, bool Visited,*/ int[] Values) {
         this.Name = Name;
         //this.Color = Color;
         Progression = 0;
         Visited = false;
         DoubleCash = false;
         this.ColorID = ColorID;
         this.Values = Values;
      }
   }

   readonly List<Properties> Places = new List<Properties> { //Name, ColorID, Values
      new Properties ("Mediterran Avenue", 0, new int[] { 2, 10, 30, 90, 160, 250}),
      new Properties ("Baltic Avenue", 0, new int[] { 4, 20, 60, 180, 320, 450}),

      new Properties ("Oriental Avenue", 1, new int[] { 6, 30, 90, 270, 400, 550}),
      new Properties ("Vermont Avenue", 1, new int[] { 6, 30, 90, 270, 400, 550}),
      new Properties ("Connecticut Avenue", 1, new int[] { 8, 40, 100, 300, 450, 600}),

      new Properties ("St. Charles Place", 2, new int[] { 10, 50, 150, 450, 625, 750}),
      new Properties ("States Avenue", 2, new int[] { 10, 50, 150, 450, 625, 750}),
      new Properties ("Virginia Avenue", 2, new int[] { 12, 60, 180, 500, 700, 900}),

      new Properties ("St. James Place", 3, new int[] { 14, 70, 200, 550, 750, 950}),
      new Properties ("Tennessee Avenue", 3, new int[] { 14, 70, 200, 550, 750, 950}),
      new Properties ("New York Avenue", 3, new int[] { 16, 80, 220, 600, 800, 1000}),

      new Properties ("Kentucky Avenue", 4, new int[] { 18, 90, 250, 700, 875, 1000}),
      new Properties ("Indiana Avenue", 4, new int[] { 18, 90, 250, 700, 875, 1000}),
      new Properties ("Illinois Avenue", 4, new int[] { 20, 100, 300, 750, 925, 1100}),

      new Properties ("Atlantic Avenue", 5, new int[] { 22, 110, 330, 800, 975, 1150}),
      new Properties ("Ventnor Avenue", 5, new int[] { 22, 110, 330, 800, 975, 1150}),
      new Properties ("Marvin Gardens", 5, new int[] { 24, 120, 360, 850, 1075, 1200}),

      new Properties ("Pacific Avenue", 6, new int[] { 26, 130, 390, 900, 1100, 1275}),
      new Properties ("North Carolina Avenue", 6, new int[] { 26, 130, 390, 900, 1100, 1275}),
      new Properties ("Pennsylvania Avenue", 6, new int[] { 28, 150, 450, 1000, 1200, 1400}),

      new Properties ("Park Place", 7, new int[] { 35, 175, 500, 1100, 1300, 1500}),
      new Properties ("Boardwalk", 7, new int[] { 50, 200, 600, 1400, 1700, 2000})
    };

   #endregion

   #region Twitch Plays

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (Command == "JAIL") {
         SubmitButton.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      else if (Command == "RECOVER") {
         StageRecoveryButton.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      else if (Command == "TOGGLE SWITCH") {
         Switch.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      else if (Command == "TOGGLE SCREEN") {
         Screen.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      else if (!Command.Any(x => "0123456789".Contains(x)) && Command != "CLEAR" && Command != "SUBMIT") {
         yield return "sendtochaterror I don't understand!";
      }
      else if (Command == "CLEAR") {
         for (int i = 0; i < 5; i++) {
            Switch.OnInteract();
            yield return new WaitForSeconds(.1f);
         }
      }
      else if (Command.Any(x => "0123456789".Contains(x)) && Solvable && Command.Length < 6) {
         for (int i = 0; i < Command.Length; i++) {
            if (Command[i] != 0) {
               for (int j = 0; j < int.Parse(Command[i].ToString()); j++) {
                  Screen.OnInteract();
                  yield return new WaitForSeconds(.1f);
               }
            }
            if (i + 1 != Command.Length) {
               Switch.OnInteract();
               yield return new WaitForSeconds(.1f);
            }
         }
         yield return new WaitForSeconds(.1f);
      }
      else if (Command == "SUBMIT") {
         SubmitButton.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      else {
         yield return "sendtochaterror I don't understand!";
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (!Solvable) {
         if (NeedToJail) {
            SubmitButton.OnInteract();
            yield return new WaitForSeconds(.1f);
         }
         yield return true;
      }
      for (int j = 0; j < 5; j++) {
         Switch.OnInteract();
         yield return new WaitForSeconds(.1f);
      }
      for (int i = 1; i < 6; i++) {
         while (SubmissionTexts[5].text != FinalAnswer[i].ToString()) {
            Screen.OnInteract();
            yield return new WaitForSeconds(.1f);
         }
         if (i != 5) {
            Switch.OnInteract();
            yield return new WaitForSeconds(.1f);
         }
      }
      SubmitButton.OnInteract();
      yield return null;
   }
}

#endregion