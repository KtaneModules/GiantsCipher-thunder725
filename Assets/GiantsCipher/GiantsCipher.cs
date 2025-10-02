using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Analytics;

public class GiantsCipher : MonoBehaviour {

	// Static data
	string[] orderedHintList = new string[]{"ACACIA", "MADMAN", "ACHING", "FALCON", "BALSAM", "MAGNET", "MAGNUM", "HADRON", "GERBIL", "EASILY", "CARING", "ABSENT", "KETTLE", "BANNER", "BASQUE", "KAZOOS", "COFFEE", "HOBBIT", "ANALOG", "BRAINS", "ERASED", "DRIVEN", "FOGRUM", "COLORS", "ATOMIC", "LUNACY", "JOYFUL", "LONDON", "INSTIL", "AUTUMN", "CONSUL", "CONVOY", "ZAGGED", "TABLES", "NAMING", "SADIST", "OBEYED", "RAISES", "VACUUM", "REBOOT", "ULTIMA", "OBTAIN", "TAXING", "NINETY", "TAPPED", "ZIPPER", "THRONE", "NEURON", "QUEBEC", "QUACKS", "WRAITH", "QUEENS", "TRIPLE", "TRASHY", "QUINOA", "ROBOTS", "WORKED", "VOWELS", "OWNING", "NOTION", "TROUGH", "VORTEX", "SPOUSE", "SORROW"};
	char[] startAlphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M' };
	string[] possibleResults = new string[] { ".X.X..XXX..X.X.", "..X..XXXXX..X..", ".X.X.X.X.X.X.X.", "X...X.XXX.X...X", "X.X.X.XXX...X.."};
    int[] baseIndicesToCheckCipherR = new int[6] { 0, 1, 5, 6, 10, 11 };
    int[] indicesToShiftCipherI = new int[9] { 12, 9, 5, 2, 4, 13, 11, 0, 12 };


    // Target & Current Data
    int resultID; // 0 Regirock - 1 Regice - 2 Registeel - 3 Regieleki - 4 Regidrago
	string currentCanvas;
	string DecryptionCipherOrder; // The order the player has to do
    string EncryptionCipherOrder; // The order the module will use to encrypt
    string SelectedKeyword;


    // Bomb & Module Variables
    public KMBombInfo bombInfos;
    public KMBombModule thisBombModule;
    public KMAudio audioSystem;
    public TextMesh keywordTextMesh;
    public List<MeshRenderer> allLeds;
    public Material LedOnMaterial, LedOffMaterial;
    public KMSelectable[] pressableButtons;
    public AudioClip regirockSound, regiceSound, registeelSound, regielekiSound, regidragoSound;


    // Logging Data - Formatting & naming from Royal_Flu$h
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;


    // Buttons gathering and GetComponents
    void Awake()
	{
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in pressableButtons)
        {
            button.OnInteract += delegate () { PatternGetsPressed(Array.IndexOf(pressableButtons, button), button); return false; };// 0 Regirock - 1 Regice - 2 Registeel - 3 Regieleki - 4 Regidrago
        }
    }


    // Puzzle Initialization
    void Start ()
	{

        Debug.LogFormat("[Giants Cipher #{0}] Starting Initialization.", moduleId);

        resultID = UnityEngine.Random.Range(0, 5);
        currentCanvas = possibleResults[resultID];

        Debug.LogFormat("[Giants Cipher #{0}] The selected final pattern is:", moduleId);
		PrintCanvasToLog();

        DetermineCipherOrder();

        EncryptMessage();

        Debug.LogFormat("[Giants Cipher #{0}] Final Keyword is {1} with result shown on the screen being:", moduleId, SelectedKeyword);
        PrintCanvasToLog();

        keywordTextMesh.text = SelectedKeyword;

        for (int i = 0; i < 15; i++)
        {
            allLeds[i].material = currentCanvas[i] == 'X' ? LedOnMaterial : LedOffMaterial;
        }
    }




	void VerifyList()
	{
		foreach (string _regi in possibleResults)
		{
			if (_regi.Length !=15)
			{
                Debug.Log("REGI " + _regi + " DOESN'T HAVE 15 CHARACTERS " + _regi.Length);
            }
		}

        Debug.Log("ALL REGIS CHECKED :D");


        char[] letters = new char[6];
		int value;

		for (int i = 0; i < orderedHintList.Length; i++) 
		{
			// Reset value
            value = 0;

            letters = orderedHintList[i].ToCharArray();
			if (letters.Length != 6)
			{
				Debug.Log("WORD " + orderedHintList[i] + " DOESN'T HAVE 6 CHARACTERS BUT HAS " + letters.Length);
				continue;
			}

			// Check every letter
			for (int j = 0; j < 6; j++)
			{
                if (!startAlphabet.Contains(letters[j]))
                {
					value += (int)Mathf.Pow(2, 5 - j);
                }
            }

			
			if (value != i)
			{
				Debug.Log("WORD " + orderedHintList[i] + " DOESN'T MATCH ITS NUMBER " + i + " BUT IS ACTUALLY " + value);
			}

		}

		Debug.Log("ALL WORDS CHECKED :D");
	}

	void DetermineCipherOrder()
	{
		// Reset order string to five empty slots
        DecryptionCipherOrder = ".....";


		// Prepare all individual variables;
		int cipherRScore = bombInfos.GetOffIndicators().Count();
        Debug.LogFormat("[Giants Cipher #{0}] Cipher R score (unlit indicators) is {1}.", moduleId, cipherRScore);

        int cipherIScore = bombInfos.GetSerialNumberNumbers().Count();
        Debug.LogFormat("[Giants Cipher #{0}] Cipher I score (Serial Number Digits) is {1}.", moduleId, cipherIScore);

        int cipherSScore = bombInfos.GetOnIndicators().Count();
        Debug.LogFormat("[Giants Cipher #{0}] Cipher S score (lit indicators) is {1}.", moduleId, cipherSScore);

        int cipherEScore = bombInfos.GetBatteryCount();
        Debug.LogFormat("[Giants Cipher #{0}] Cipher E score (Batteries) is {1}.", moduleId, cipherEScore);

        int cipherDScore = bombInfos.GetPortPlateCount();
        Debug.LogFormat("[Giants Cipher #{0}] Cipher D score (Port plates) is {1}.", moduleId, cipherDScore);


        // Sort them in descending order.
        int[] _numberOrder = new int[5] { cipherRScore, cipherIScore, cipherSScore, cipherEScore, cipherDScore};
		Array.Sort(_numberOrder);
        _numberOrder = _numberOrder.Reverse().ToArray();


        // Then retro-determine the placement of each Cipher, giving priority to R I S E D in this order.
        // For that, loop through the number order, skipping Ciphers that already exist, and assign them to the CipherOrder
        for (int i = 0; i < 5; i++)
		{
			int _cipherScore = _numberOrder[i];

			// If we haven't already assigned Cipher R and Cipher R is the correct number, it HAS to be the correct choice
			// Since we go in the order R I S E D this is guaranteed to be correct.
			if (!DecryptionCipherOrder.Contains("R") && cipherRScore == _cipherScore)
			{
				DecryptionCipherOrder = DecryptionCipherOrder.Remove(i, 1).Insert(i, "R");
            }
			else if (!DecryptionCipherOrder.Contains("I") && cipherIScore == _cipherScore)
            {
                DecryptionCipherOrder = DecryptionCipherOrder.Remove(i, 1).Insert(i, "I");
            }
            else if (!DecryptionCipherOrder.Contains("S") && cipherSScore == _cipherScore)
            {
                DecryptionCipherOrder = DecryptionCipherOrder.Remove(i, 1).Insert(i, "S");
            }
			else if (!DecryptionCipherOrder.Contains("E") && cipherEScore == _cipherScore)
            {
                DecryptionCipherOrder = DecryptionCipherOrder.Remove(i, 1).Insert(i, "E");
            }
			else if (!DecryptionCipherOrder.Contains("D") && cipherDScore == _cipherScore)
            {
                DecryptionCipherOrder = DecryptionCipherOrder.Remove(i, 1).Insert(i, "D");
            }

        }

        Debug.LogFormat("[Giants Cipher #{0}] The final sorted Cipher order for DECRYPTION is {1}", moduleId, DecryptionCipherOrder);

        EncryptionCipherOrder = new string(DecryptionCipherOrder.Reverse().ToArray());

        Debug.LogFormat("[Giants Cipher #{0}] The module will now encrypt the canvas using reversed order {1}", moduleId, EncryptionCipherOrder);
    }

    void EncryptMessage()
    {
        char[] _encryptionOrder = EncryptionCipherOrder.ToCharArray();

        foreach (char _nextCipher in _encryptionOrder)
        {
            switch (_nextCipher)
            {
                case 'R':
                    EncryptCipherR();
                    break;


                case 'I':
                    EncryptCipherI();
                    break;


                case 'S':
                    EncryptCipherS();
                    break;


                case 'E':
                    EncryptCipherE();
                    break;


                case 'D':
                    EncryptCipherD(5 - DecryptionCipherOrder.IndexOf('D')); // Power is in range 1-5
                    break;
            }
        }
    }

    void EncryptCipherR()
    {
        // To Encrypt using R:
        // Determine which side (Left Right) to use
        // Note down the current 2x3 state as a binary number 0-63
        // Take the word in orderedHintList that corresponds to that binary number
        // Replace the side with random LED states

        Debug.LogFormat("[Giants Cipher #{0}] Beginning R Encryption.", moduleId);

        var _allIndicators = String.Concat(bombInfos.GetIndicators().ToArray()).ToUpperInvariant();

        // This translates to "does the string _allIndicators contain at least a character from the pool R E G I" ?
        bool _shouldReplaceLeft = Regex.IsMatch(_allIndicators, "[REGI]");
        
        if (_shouldReplaceLeft)
        {
            Debug.LogFormat("[Giants Cipher #{0}] Indicator contaning a letter in 'EGIR' found, replacing the left side.", moduleId);
        }
        else
        {
            Debug.LogFormat("[Giants Cipher #{0}] Indicator contaning a letter in 'EGIR' not found, replacing the right side.", moduleId);
        }


        int _selectedSideScore = 0;

        // Use the baseIndicesToCheckCipherR in order
        // If we should replace right however, offset every index by +3 to go to the right side
        int _rightOffset = _shouldReplaceLeft ? 0 : 3;
        char _currentLight;
        string _sideLinearRepresentation = "";

        for (int i = 0; i < 6; i ++)
        {
            _currentLight = currentCanvas[baseIndicesToCheckCipherR[i] + _rightOffset];
            _sideLinearRepresentation += _currentLight;
            if (_currentLight == 'X')
            {
                _selectedSideScore += (int)Mathf.Pow(2, 5 - i);
            }

            // Randomly scramble the Canvas, the information is stored in the Keyword so we don't need it anymore and can randomize it
            if (UnityEngine.Random.value > 0.5)
            {
                FlipLightInCanvas(baseIndicesToCheckCipherR[i] + _rightOffset);
                Debug.LogFormat("[Giants Cipher #{0}] Randomly decided to flip light with index {1}", moduleId, baseIndicesToCheckCipherR[i] + _rightOffset);
            }
        }

        SelectedKeyword = orderedHintList[_selectedSideScore];

        Debug.LogFormat("[Giants Cipher #{0}] Selected the Keyword {1}, with representation (in line) {2}", moduleId, SelectedKeyword, _sideLinearRepresentation);
        Debug.LogFormat("[Giants Cipher #{0}] R Encryption finished, new Canvas state is:", moduleId);
        PrintCanvasToLog();
    }

    void EncryptCipherI()
    {
        // To Encrypt using I:
        // Take the table in the manual,
        // but swap 8 into 7, 7 into 6, 6 into 5, ... and 1 into 8

        Debug.LogFormat("[Giants Cipher #{0}] Beginning I Encryption.", moduleId);
        Debug.LogFormat("[Giants Cipher #{0}] Swapping 8 into 7, 7 into 6... and 1 into 8.", moduleId);


        int _currentLightIndex;
        char _previousLight = currentCanvas[indicesToShiftCipherI[0]];
        char _currentLight;

        for (int i = 1; i < 9; i ++)
        {
            _currentLightIndex = indicesToShiftCipherI[i];
            _currentLight = currentCanvas[_currentLightIndex];
            currentCanvas = currentCanvas.Remove(_currentLightIndex, 1).Insert(_currentLightIndex, _previousLight.ToString());

            _previousLight = _currentLight;
        }

        Debug.LogFormat("[Giants Cipher #{0}] I Encryption finished, new Canvas state is:", moduleId);
        PrintCanvasToLog();
    }

    void EncryptCipherS()
    {
        // To Encrypt using S:
        // Read the line as binary 0-31
        // Add 32 until the number is a multiple of five
        // Divide by 5
        // Set the line as the result when read in binary
        // Yes, all numbers from 0-31 get encrypted to another unique number within 0-31 without loss

        Debug.LogFormat("[Giants Cipher #{0}] Beginning S Encryption.", moduleId);

        
        string topRow = currentCanvas.Remove(5, 10);
        string middleRow = currentCanvas.Remove(10, 5).Remove(0, 5);
        string bottomRow = currentCanvas.Remove(0, 10);

        topRow = ComputeCipherSNewRow(topRow, "Top Row");
        middleRow = ComputeCipherSNewRow(middleRow, "Middle Row");
        bottomRow = ComputeCipherSNewRow(bottomRow, "Bottom Row");

        currentCanvas = topRow + middleRow + bottomRow;

        Debug.LogFormat("[Giants Cipher #{0}] S Encryption finished, new Canvas state is:", moduleId);
        PrintCanvasToLog();

    }

    string ComputeCipherSNewRow(string row, string rowPrintName)
    {
        // Step 1 : Convert to Binary
        char _currentLight;
        int binaryResult = 0;

        for (int i = 0; i < 5; i++)
        {
            _currentLight = row[i];
            if (_currentLight == 'X')
            {
                binaryResult += (int)Mathf.Pow(2, 4 - i);
            }
        }

        Debug.LogFormat("[Giants Cipher #{0}] S Encryption - {1}: Decrypted binary value is {2}", moduleId, rowPrintName, binaryResult);

        // Step 2 : Add 32 until it's a multiplier of 5, then divide by 5

        while (binaryResult % 5 != 0)
        {
            binaryResult += 32;
        }

        binaryResult /= 5;

        Debug.LogFormat("[Giants Cipher #{0}] S Encryption - {1}: Encrypted binary value is {2}", moduleId, rowPrintName, binaryResult);

        // Step 3 : Convert back to binary

        row = "";

        for (int i = 4; i >= 0; i --)
        {
            if (binaryResult >= Mathf.Pow(2, i))
            {
                row += 'X';
                binaryResult -= (int)Mathf.Pow(2, i);
            }
            else
            {
                row += '.';
            }
        }

        Debug.LogFormat("[Giants Cipher #{0}] S Encryption - {1}: Encrypted binary representation is {2}", moduleId, rowPrintName, row);


        return row;
    }

    void EncryptCipherE()
    {
        // To Encrypt using E:
        // Just do the XOR again, it's a flip-flop

        // . . X . .
        // . X X X .
        // X . X . X
        // => 2 6 7 8 10 12 14

        Debug.LogFormat("[Giants Cipher #{0}] Beginning E Encryption.", moduleId);

        FlipLightInCanvas(2);
        FlipLightInCanvas(6);
        FlipLightInCanvas(7);
        FlipLightInCanvas(8);
        FlipLightInCanvas(10);
        FlipLightInCanvas(12);
        FlipLightInCanvas(14);

        Debug.LogFormat("[Giants Cipher #{0}] E Encryption finished, new Canvas state is:", moduleId);
        PrintCanvasToLog();
    }

    void EncryptCipherD(int cipherPower) // Power is in range 1-5
    {
        // To Encrypt using D:
        // Do the steps in reverse order (3 > 2 > 1)
        // Step 1 encryption is just Step 5 decryption
        // Shift to the left instead of shifting to the right
        // Encrypting using XOR is just doing it again, like Cipher E
        // Rotate the center square 180 again
        // Step 5 encryption is just Step 1

        if (cipherPower >= 5)
        {
            // Do Step 5
            CipherDRotateClockwise();
            Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 5 finished, new Canvas state is:", moduleId);
            PrintCanvasToLog();
        }

        if (cipherPower >= 4)
        {
            // Do Step 4


            string oldCanvas = currentCanvas;

            // Set top row
            char[] _newLine = new char[3] { oldCanvas[13], oldCanvas[12], oldCanvas[11] };
            currentCanvas = currentCanvas.Remove(1, 3).Insert(1, new string(_newLine));

            // Set middle row
            _newLine = new char[3] { oldCanvas[8], oldCanvas[7], oldCanvas[6] };
            currentCanvas = currentCanvas.Remove(6, 3).Insert(6, new string(_newLine));

            // Set bottom row
            _newLine = new char[3] { oldCanvas[3], oldCanvas[2], oldCanvas[1] };
            currentCanvas = currentCanvas.Remove(11, 3).Insert(11, new string(_newLine));

            Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 4 finished, new Canvas state is:", moduleId);
            PrintCanvasToLog();
        }

        if (cipherPower >= 3)
        {
            // Do Step 3

            // . X X X .
            // X . . . X
            // . X X X . 
            // => 1 2 3 5 9 11 12 13
            FlipLightInCanvas(1);
            FlipLightInCanvas(2);
            FlipLightInCanvas(3);
            FlipLightInCanvas(5);
            FlipLightInCanvas(9);
            FlipLightInCanvas(11);
            FlipLightInCanvas(12);
            FlipLightInCanvas(13);

            Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 3 finished, new Canvas state is:", moduleId);
            PrintCanvasToLog();
        }

        if (cipherPower >= 2)
        {
            // Do Step 2

            // Shift the same amount as cipherPower to the LEFT
            Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 2: Shifting a total of {1} times.", moduleId, cipherPower);

            string middleRow = currentCanvas.Remove(10, 5).Remove(0, 5);


            for (int i = 0; i < cipherPower; i++)
            {
                middleRow = middleRow.Remove(0, 1) + middleRow[0];
            }

            currentCanvas = currentCanvas.Remove(5, 5).Insert(5, middleRow);

            Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 2 finished, new Canvas state is:", moduleId);
            PrintCanvasToLog();
        }

        // Do Step 1
        CipherDRotateCounterclockwise();
        Debug.LogFormat("[Giants Cipher #{0}] D Encryption - Step 1 finished, new Canvas state is:", moduleId);
        PrintCanvasToLog();
    }

    void CipherDRotateClockwise()
    {
        // Start
        // 01234
        // 56789
        // ABCDE

        // After Clockwise rotation three times
        // BA501
        // C6782
        // DE943
        char[] _newCanvas = new char[15] { currentCanvas[11], currentCanvas[10], currentCanvas[5], currentCanvas[0], currentCanvas[1], currentCanvas[12], currentCanvas[6], currentCanvas[7], currentCanvas[8], currentCanvas[2], currentCanvas[13], currentCanvas[14], currentCanvas[9], currentCanvas[4], currentCanvas[3] };
        currentCanvas = new string(_newCanvas);

    }

    void CipherDRotateCounterclockwise()
    {
        // Start
        // 01234
        // 56789
        // ABCDE

        // After Counter-Clockwise rotation three times
        // 349ED
        // 2678C
        // 105AB
        char[] _newCanvas = new char[15] { currentCanvas[3], currentCanvas[4], currentCanvas[9], currentCanvas[14], currentCanvas[13], currentCanvas[2], currentCanvas[6], currentCanvas[7], currentCanvas[8], currentCanvas[12], currentCanvas[1], currentCanvas[0], currentCanvas[5], currentCanvas[10], currentCanvas[11] };
        currentCanvas = new string(_newCanvas);
    }

    void FlipLightInCanvas(int lightIndex)
    {
        char _value = currentCanvas[lightIndex];
        _value = _value == '.' ? 'X' : '.';

        currentCanvas = currentCanvas.Remove(lightIndex, 1).Insert(lightIndex, _value.ToString());

    }

    void PrintCanvasToLog(bool useInternalCanvas = true, string CanvasToPrint = "")
	{

        if (useInternalCanvas)
        {
            CanvasToPrint = currentCanvas;
        }
		string individualLine = CanvasToPrint.Remove(5, 10);
        Debug.LogFormat("[Giants Cipher #{0}] {1}", moduleId, individualLine);

        individualLine = CanvasToPrint.Remove(10, 5).Remove(0, 5);
        Debug.LogFormat("[Giants Cipher #{0}] {1}", moduleId, individualLine);

        individualLine = CanvasToPrint.Remove(0, 10);
        Debug.LogFormat("[Giants Cipher #{0}] {1}", moduleId, individualLine);
    }


    void PatternGetsPressed(int patternIndex, KMSelectable pressedButton) // 0 Regirock - 1 Regice - 2 Registeel - 3 Regieleki - 4 Regidrago
    {
        if (moduleSolved)
        { return; }
        pressedButton.AddInteractionPunch(0.6f);

        Debug.LogFormat("[Giants Cipher #{0}] Pressed button with pattern:", moduleId);
        PrintCanvasToLog(false, possibleResults[patternIndex]);

        if (patternIndex == resultID)
        {

            HandleSolve();
        }
        else
        {
            HandleStrike();
        }
    }

    void HandleSolve()
    {
        Debug.LogFormat("[Giants Cipher #{0}] That is Correct! Module solved!", moduleId);

        AudioClip _soundToPlay = null;

        switch (resultID)
        {
            case 0:
                _soundToPlay = regirockSound;
                break;

            case 1:
                _soundToPlay = regiceSound;
                break;

            case 2:
                _soundToPlay = registeelSound;
                break;

            case 3:
                _soundToPlay = regielekiSound;
                break;

            case 4:
                _soundToPlay = regidragoSound;
                break;
        }

        audioSystem.PlaySoundAtTransform(_soundToPlay.name, transform);

        keywordTextMesh.text = "WELL DONE";

        StartCoroutine(LightSolvePatternLights());

        moduleSolved = true;
        thisBombModule.HandlePass();
    }

    void HandleStrike()
    {
        Debug.LogFormat("[Giants Cipher #{0}] That is Wrong! !i!i! GET STRIKED !i!i!", moduleId);
        thisBombModule.HandleStrike();
    }


    IEnumerator LightSolvePatternLights()
    {
        // Entire "Turn Off then Turn On" sequence to hide starting information
        // Adds some cool solve feedback, and allow Souvenir questions

        // Determine which lights should be turned on in advance
        string correctResult = possibleResults[resultID];
        int[] lightsToTurnOnID = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
        int numberOfLightsRegistered = 0;

        // Turn off all lights
        // While we're doing a loop, register the Light IDs
        for (int i = 0; i < 15; i++)
        {
            allLeds[i].material = LedOffMaterial;
            if (correctResult[i] == 'X')
            {
                lightsToTurnOnID[numberOfLightsRegistered] = i;
                numberOfLightsRegistered++;
            }
        }

        yield return new WaitForSeconds(0.35f);


        // Turn on each light, in reading order, one by one
        for (int i = 0; i < 7; i++)
        {
            allLeds[lightsToTurnOnID[i]].material = LedOnMaterial;
            yield return new WaitForSeconds(0.15f);
        }

    }


#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"“!{0} Press 1” to press the first Pattern shown in the manual in reading order. Valid numbers are 1-5.";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        // Credit to Royal_Flu$h for this line 
        var commandParts = command.ToLowerInvariant().Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        // We only accept submissions with press and one number
        if (commandParts.Length != 2)
        {
            return null;
        }

        if (!commandParts[0].Equals("press"))
        {
            return null;
        }

        int _pressedIndex = int.Parse(commandParts[1]);

        if (_pressedIndex < 1 || _pressedIndex > 5)
        {
            return null;
        }

        // -1 since we accept 1-5 but the indices are 0-4
        return new KMSelectable[] { pressableButtons[_pressedIndex - 1] };
    }

    // Auto-solve if Twitch Plays needs to force a solve
    KMSelectable[] TwitchHandleForcedSolve()
    {
        moduleSolved = true;

        return new KMSelectable[] { pressableButtons[resultID] };
    }

}
