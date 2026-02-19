using System.Runtime.InteropServices;
using System.Windows.Forms;
using UI_space;

namespace Input_Space {
    public class InputManager {    
        public const int NONE_INPUT = 0;
        public const int KEYBOARD_A_INPUT = 1;
        public const int KEYBOARD_B_INPUT = 2;
        public const int JOYSTICK_0_INPUT = 3;
        public const int JOYSTICK_1_INPUT = 4;

        public const int DEFAULT = 0;
        public const int PLAYER_A = 1;
        public const int PLAYER_B = 2;

        private static Dictionary<int, Dictionary<string, int>> keysTranslationMap = new Dictionary<int, Dictionary<string, int>> {
            { -1, new Dictionary<string, int> {
                    { "A", 0 },
                    { "B", 1 },
                    { "C", 2 },
                    { "D", 3 },
                    { "LB", 4 },
                    { "LT", 5 },
                    { "RB", 6 },
                    { "RT", 7 },
                    { "Up", 8 },
                    { "Down", 9 },
                    { "Left", 11 },
                    { "Right", 10 },
                    { "Start", 12 },
                    { "Select", 13 },
                }
            },
            { 1, new Dictionary<string, int> {
                    { "A", 0 },
                    { "B", 1 },
                    { "C", 2 },
                    { "D", 3 },
                    { "LB", 4 },
                    { "LT", 5 },
                    { "RB", 6 },
                    { "RT", 7 },
                    { "Up", 8 },
                    { "Down", 9 },
                    { "Left", 10 },
                    { "Right", 11 },
                    { "Start", 12 },
                    { "Select", 13 },
                }
            }
        };

        private const int maxButtonIndex = 14;

        public static bool autoDetectDevice;
        private static int[] inputDevice = new int[3];
        public static int[] buttonState = new int[3];
        public static int[] buttonLastState = new int[3];

        public static bool anyKey => InputManager.buttonState[DEFAULT] > 0;
        public static bool anyKeyA => InputManager.buttonState[PLAYER_A] > 0;
        public static bool anyKeyB => InputManager.buttonState[PLAYER_B] > 0;


        // Mapping para teclado e mouse
        private static Dictionary<Keys, int> keyMapA = new Dictionary<Keys, int> {
            { Keys.Z, 0 },      // A
            { Keys.X, 1 },      // B
            { Keys.A, 2 },      // X
            { Keys.S, 3 },      // Y
            { Keys.F, 4 },      // L
            { Keys.V, 5 },      // LT
            { Keys.D, 6 },      // R
            { Keys.C, 7 },      // RT
            { Keys.Up, 8 },     // Up
            { Keys.Down, 9 },   // Down
            { Keys.Left, 10 },  // Left
            { Keys.Right, 11 }, // Right
            { Keys.Enter, 12 }, // Start
            { Keys.Space, 13 }, // Select
        };
        private static Dictionary<Keys, int> keyMapB = new Dictionary<Keys, int> {
            { Keys.Q, 0 },      // A
            { Keys.W, 1 },      // B
            { Keys.D1, 2 },     // X
            { Keys.D2, 3 },     // Y
            { Keys.D4, 4 },     // L
            { Keys.R, 5 },      // LT
            { Keys.D3, 6 },     // R
            { Keys.E, 7 },      // RT
            { Keys.NumPad8, 8 },   // Up
            { Keys.NumPad2, 9 },   // Down
            { Keys.NumPad4, 10 },  // Left
            { Keys.NumPad6, 11 },  // Right
            { Keys.Add, 12 },      // Start
            { Keys.Subtract, 13 }, // Select
        };
        private static Dictionary<int, int> joystickMap = new Dictionary<int, int> {
            { 0x1000, 0 },      // A
            { 0x2000, 1 },      // B
            { 0x4000, 2 },      // X
            { 0x8000, 3 },      // Y
            { 0x0100, 4 },      // L
            { 0x0400, 5 },      // LT
            { 0x0200, 6 },      // R
            { 0x0800, 7 },      // RT
            { 0x0001, 8 },      // Up
            { 0x0002, 9 },      // Down
            { 0x0004, 10 },     // Esquerda
            { 0x0008, 11 },     // Direita
            { 0x0010, 12 },     // Start
            { 0x0020, 13 },     // Select
        };

        // Buffer de inputs para os últimos 240 frames (4 segundos)
        private const int BufferSize = 240;
        private static LinkedList<int>[] buffers = new LinkedList<int>[3];

        // Construtor
        public InputManager(bool autoDetectDevice = true) {
            InputManager.autoDetectDevice = autoDetectDevice;
            for (int i = 0; i < 3; i++) {
                InputManager.buttonState[i] = 0b0;
                InputManager.buttonLastState[i] = 0b0;
            }

            InputManager.buffers = new LinkedList<int>[] {new LinkedList<int>(), new LinkedList<int>(), new LinkedList<int>()};
        }

        // Behaviour
        public static void Update() {
            // Altera automaticamente o dispositivo de entrada
            if (autoDetectDevice && JoystickInput.IsJoystickConnected(0) && JoystickInput.IsJoystickConnected(1))  {
                inputDevice[0] = NONE_INPUT;
                inputDevice[1] = JOYSTICK_0_INPUT;
                inputDevice[2] = JOYSTICK_1_INPUT;
            } else if (autoDetectDevice && JoystickInput.IsJoystickConnected(0)) {
                inputDevice[0] = NONE_INPUT;
                inputDevice[1] = JOYSTICK_0_INPUT;
                inputDevice[2] = KEYBOARD_A_INPUT;
            } else {
                inputDevice[0] = NONE_INPUT;
                inputDevice[1] = KEYBOARD_A_INPUT;
                inputDevice[2] = KEYBOARD_B_INPUT;
            }

            // Lê o estado atual dos dispositivos de entrada
            int[] currentInput =  new int[3] {0, 0, 0};
            for (int i = 1; i < 3; i++) {
                if (inputDevice[i] == KEYBOARD_A_INPUT) {
                    currentInput[i] = RawInput.ReadKeyboardState(keyMapA);
                }
                else if (inputDevice[i] == KEYBOARD_B_INPUT) {
                    currentInput[i] = RawInput.ReadKeyboardState(keyMapB);
                }
                else if (inputDevice[i] == JOYSTICK_0_INPUT) {
                    currentInput[i] = JoystickInput.ReadJoystickState(joystickMap, dwUserIndex: 0);
                }
                else if (inputDevice[i] == JOYSTICK_1_INPUT) {
                    currentInput[i] = JoystickInput.ReadJoystickState(joystickMap, dwUserIndex: 1);
                }
            }

            // Atualiza o estado dos botões
            for (int j = 2; j >= 0; j--) {
                InputManager.buttonLastState[j] = InputManager.buttonState[j];
                InputManager.buttonState[j] = 0;

                // Processa o estado dos botões default
                if (j == 0) InputManager.buttonState[0] = InputManager.buttonState[1] | InputManager.buttonState[2];
                else {
                    for (int i = 0; i < InputManager.maxButtonIndex; i++)
                        if ((currentInput[j] & (1 << i)) != 0) 
                            InputManager.buttonState[j] |= 1 << i;
                }

                // Adiciona o estado atual do botão no buffer
                if (buffers[j].Count >= BufferSize) buffers[j].RemoveFirst();
                buffers[j].AddLast(InputManager.buttonState[j]);
            }


        }
        public static void UpdateAI() {
            if (Program.AI_playerA & InputManager.anyKeyA) Program.AI_playerA = false;
            if (Program.AI_playerB & InputManager.anyKeyB) Program.AI_playerB = false;
        }
        public static void ResetAI() {
            Program.AI_playerA = true;
            Program.AI_playerB = true;
        }
        
        // Key Detection
        public static bool Key_hold(string key, int player = DEFAULT, int facing = 1) {
            return (buttonState[player] & (1 << keysTranslationMap[facing][key])) != 0;
        }
        public static bool Key_down(string key, int player = DEFAULT, int facing = 1) {
            return (buttonState[player] & (1 << keysTranslationMap[facing][key])) != 0 && (buttonLastState[player] & (1 << keysTranslationMap[facing][key])) == 0;
        }
        public static bool Key_up(string key, int player = DEFAULT, int facing = 1) {
            return (buttonState[player] & (1 << keysTranslationMap[facing][key])) == 0 && (buttonLastState[player] & (1 << keysTranslationMap[facing][key])) != 0;
        }
        public static bool Key_change(string key, int player = DEFAULT, int facing = 1) {
            return (buttonState[player] & (1 << keysTranslationMap[facing][key])) != (buttonLastState[player] & (1 << keysTranslationMap[facing][key]));
        }
        public static bool Key_sequence_press(string rawSequenceString, int maxFrames, int player = DEFAULT, int facing = 1, bool flexEntry = true, bool flexTransition = true) {
            int[] sequence = rawSequenceString.Split(' ').Select(key => keysTranslationMap[facing][key]).ToArray();
            List<int> bufferList = buffers[player].ToList();

            int currentIndex = sequence.Length - 1;
            int currentFrame = bufferList.Count - 1;

            // Verificação da sequência (de trás para frente)
            for (int i = currentIndex; i >= 0; i--) {
                bool found = false;

                // Busca o input atual na janela de maxFrames
                for (int j = currentFrame; j >= Math.Max(0, currentFrame - maxFrames); j--) {
                    bool isButtonPressed = (bufferList[j] & (1 << sequence[i])) != 0;
                    bool wasButtonReleased = (j == 0) || (bufferList[j - 1] & (1 << sequence[i])) == 0;

                    if (i == 0 && flexEntry) {
                        if (isButtonPressed) {
                            found = true;
                            currentFrame = j - 1;
                            break;
                        }
                    } else {
                        if (isButtonPressed && wasButtonReleased) {
                            found = true;
                            currentFrame = j - 1;
                            break;
                        }
                    }
                }

                if (!found) return false;

                if (!flexTransition && i > 0) {
                    for (int j = currentFrame; j >= Math.Max(0, currentFrame - maxFrames); j--) {
                        // Se algum botão fora da sequência estiver pressionado, invalida
                        if ((bufferList[j] & ~(1 << sequence[i])) != 0) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        public static bool Key_press(string key, int player = DEFAULT, int facing = 1, int input_window = -1) {
            List<int> bufferList = buffers[player].ToList();
            for (int i = bufferList.Count() - 1; i > (bufferList.Count() - (input_window == -1 ? Config.input_window_time : input_window)); i--) {
                if ((bufferList[i] & (1 << keysTranslationMap[facing][key])) != 0 && (bufferList[i-1] & (1 << keysTranslationMap[facing][key])) == 0) {
                    return true;
                };
            }
            return false;
        }
        
        // Key Manipulation
        public static void SetKey(string key, int player, int facing = 1) {
            if (key == "") return;
            if (key.Contains("*")) InputManager.buttonState[player] = 0; // Clear the input first
            
            foreach (var k in key.Split(' ')) {
                if (keysTranslationMap[facing].ContainsKey(k)) {
                    InputManager.buttonState[player] |= 1 << keysTranslationMap[facing][k];
                    InputManager.buffers[player].Last.Value = InputManager.buttonState[player];
                }
            }
        }

    public static class RawInput
        {
            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(Keys vKey);

            public static int ReadKeyboardState(Dictionary<Keys, int> keyMapA)
            {
                int state = 0;

                foreach (var key in keyMapA)
                {
                    if ((GetAsyncKeyState(key.Key) & 0x8000) != 0)
                    {
                        state |= 1 << key.Value;
                    }
                }

                return state;
            }
        }

    public class JoystickInput {
        [DllImport("xinput1_3.dll")]
        private static extern int XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

        [DllImport("xinput1_3.dll")]
        private static extern int XInputSetState(int dwUserIndex, ref XINPUT_VIBRATION pVibration);

        private const int ERROR_DEVICE_NOT_CONNECTED = 1167;

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_STATE {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_GAMEPAD {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_VIBRATION {
            public ushort wLeftMotorSpeed;
            public ushort wRightMotorSpeed;
        }

        public static int ReadJoystickState(Dictionary<int, int> joystickMap, int dwUserIndex = 0) {
            XINPUT_STATE state;
            if (XInputGetState(dwUserIndex, out state) != 0) return 0;

            int stateValue = 0;
            const int threshold = 16384; // 50% do analógico
            var g = state.Gamepad;

            // 1. Processa direções (combina DPad + Analógico)
            stateValue |= ((g.wButtons & 0x0001) != 0 | (g.sThumbLY > threshold)) ? (1 << joystickMap[0x0001]) : 0; // Up
            stateValue |= ((g.wButtons & 0x0002) != 0 | (g.sThumbLY < -threshold)) ? (1 << joystickMap[0x0002]) : 0; // Down
            stateValue |= ((g.wButtons & 0x0004) != 0 | (g.sThumbLX < -threshold)) ? (1 << joystickMap[0x0004]) : 0; // Left
            stateValue |= ((g.wButtons & 0x0008) != 0 | (g.sThumbLX > threshold)) ? (1 << joystickMap[0x0008]) : 0; // Right

            // 2. Processa botões conforme o mapping original
            stateValue |= (g.wButtons & 0x1000) != 0 ? (1 << joystickMap[0x1000]) : 0; // A
            stateValue |= (g.wButtons & 0x2000) != 0 ? (1 << joystickMap[0x2000]) : 0; // B
            stateValue |= (g.wButtons & 0x4000) != 0 ? (1 << joystickMap[0x4000]) : 0; // X
            stateValue |= (g.wButtons & 0x8000) != 0 ? (1 << joystickMap[0x8000]) : 0; // Y
            stateValue |= (g.wButtons & 0x0100) != 0 ? (1 << joystickMap[0x0100]) : 0; // LB
            stateValue |= (g.wButtons & 0x0200) != 0 ? (1 << joystickMap[0x0200]) : 0; // RB
            stateValue |= g.bLeftTrigger > 0 ? (1 << joystickMap[0x0400]) : 0;         // LT
            stateValue |= g.bRightTrigger > 0 ? (1 << joystickMap[0x0800]) : 0;        // RT
            stateValue |= (g.wButtons & 0x0010) != 0 ? (1 << joystickMap[0x0010]) : 0; // Start
            stateValue |= (g.wButtons & 0x0020) != 0 ? (1 << joystickMap[0x0020]) : 0; // Select

            return stateValue;
        }

        public static bool IsJoystickConnected(int dwUserIndex = 0)
        {
            XINPUT_STATE state;
            int result = XInputGetState(dwUserIndex, out state);

            return result == 0;
        }
    }

    }
}
