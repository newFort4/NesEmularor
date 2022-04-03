using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using NesEmulator.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.MacOS;

namespace NesEmulator
{
    public partial class MainWindow : NSWindow
	{
        #region Computed Properties
        public MonoMacGameView Game { get; set; }

        private CPU cpu;
        #endregion

        #region Constructors

        // Called when created from unmanaged code
        public MainWindow (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindow (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

        #endregion

        #region Overrides
        public override void KeyDown(NSEvent theEvent)
        {
            // handle key down events here
            const ushort keyAddress = 0xFF;

            switch (theEvent.CharactersIgnoringModifiers)
            {
                case "w":
                    cpu.WriteMemory(keyAddress, 0x77);
                    break;
                case "a":
                    cpu.WriteMemory(keyAddress, 0x61);
                    break;
                case "s":
                    cpu.WriteMemory(keyAddress, 0x73);
                    break;
                case "d":
                    cpu.WriteMemory(keyAddress, 0x64);
                    break;
                default:
                    break;
            }
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Game = new MonoMacGameView(ContentView.Frame);
            ContentView = Game;
            Game.OpenGLContext.View = Game;

            var random = new Random();
            var screenState = Enumerable
                .Range(0, 32 * 3 * 32)
                .Select(x => (byte)x)
                .ToArray();

            Game.Load += (sender, e) =>
            {
                // ToDo: Initialize settings, load textures and sounds here
                cpu = new("Alter_Ego.nes");

                cpu.Reset();

                //screenState = Array.Empty<byte>();
                //foreach (var tile in Enumerable.Repeat(0, 1).Select(x => Core.Frame.ShowFrame(cpu.Bus.ROM.CHRROM, 1, x).Data))
                //{
                //    screenState = screenState.Concat(tile).ToArray();
                //}
                var tileFrame = Core.Frame.ShowFrame(cpu.Bus.ROM.CHRROM, 1, 255);
                screenState = tileFrame.Data;
            };

            Game.Resize += (sender, e) =>
            {
                GL.Viewport(0, 0, Game.Size.Width, Game.Size.Height);
            };

            #region Initializating of static logic
            Color GetColor(byte data)
            {
                switch (data)
                {
                    case 0:
                        return Color.Black;
                    case 1:
                        return Color.White;
                    case 2:
                    case 9:
                        return Color.DarkGray;
                    case 3:
                    case 10:
                        return Color.Red;
                    case 4:
                    case 11:
                        return Color.Green;
                    case 5:
                    case 12:
                        return Color.Blue;
                    case 6:
                    case 13:
                        return Color.Magenta;
                    case 7:
                    case 14:
                        return Color.Yellow;
                    default:
                        return Color.Cyan;
                }
            }

            void RenderGame()
            {
                var pointer = (0, 240);

                for (var i = 0; i < screenState.Length; i += 3)
                {
                    var color = Color.FromArgb(screenState[i], screenState[i + 1], screenState[i + 2]);
                    DrawPixel(pointer.Item1, pointer.Item2, color);
                    pointer = (pointer.Item1 + 1, pointer.Item2);

                    if (((i / 3) + 1) % 256 == 0)
                    {
                        pointer = (0, pointer.Item2 - 1);
                    }
                }
            }
            #endregion

            Game.UpdateFrame += (sender, e) =>
            {
                // ToDo: Add any game logic or physics
                //var frameIdx = 0;

                //var frame = screenState;

                //for (var i = (ushort)0x0200; i < 0x0600; i++)
                //{
                //    var colorIdx = cpu.ReadMemory(i);
                //    var color = GetColor(colorIdx);
                //    var (r, g, b) = (color.R, color.G, color.B);

                //    if (frame[frameIdx] != r || frame[frameIdx + 1] != g || frame[frameIdx + 2] != b)
                //    {
                //        frame[frameIdx] = r;
                //        frame[frameIdx + 1] = g;
                //        frame[frameIdx + 2] = b;
                //    }

                //    frameIdx += 3;
                //}
            };

            void DrawPixel(int x, int y, Color color)
            {
                GL.Begin(BeginMode.Polygon);

                GL.Color3(color);

                GL.Vertex2(x, y);
                GL.Vertex2(x, y + 1);
                GL.Vertex2(x + 1, y + 1);
                GL.Vertex2(x + 1, y);

                GL.End();
            };

            Game.RenderFrame += (sender, e) =>
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.MatrixMode(MatrixMode.Projection);

                GL.LoadIdentity();
                GL.Ortho(0, 256, 0, 240, 0.0, 1.0);

                RenderGame();
            };

            Game.Run(60);

            var cpuTask = new Task(() =>
            {
            });

            cpuTask.Start();
        }
        #endregion
    }
}
