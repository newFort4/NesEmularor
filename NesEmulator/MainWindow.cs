﻿using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private Joypad joypad;
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
            switch (theEvent.CharactersIgnoringModifiers)
            {
                case "w":
                    joypad.SetButtonPressedStatus(JoypadButton.Up, true);
                    break;
                case "a":
                    joypad.SetButtonPressedStatus(JoypadButton.Left, true);
                    break;
                case "s":
                    joypad.SetButtonPressedStatus(JoypadButton.Down, true);
                    break;
                case "d":
                    joypad.SetButtonPressedStatus(JoypadButton.Right, true);
                    break;
                case "v":
                    joypad.SetButtonPressedStatus(JoypadButton.Start, true);
                    break;
                case "b":
                    joypad.SetButtonPressedStatus(JoypadButton.Select, true);
                    break;
                case "j":
                    joypad.SetButtonPressedStatus(JoypadButton.ButtonA, true);
                    break;
                case "k":
                    joypad.SetButtonPressedStatus(JoypadButton.ButtonB, true);
                    break;
                default:
                    break;
            }
        }

        public override void KeyUp(NSEvent theEvent)
        {
            switch (theEvent.CharactersIgnoringModifiers)
            {
                case "w":
                    joypad.SetButtonPressedStatus(JoypadButton.Up, false);
                    break;
                case "a":
                    joypad.SetButtonPressedStatus(JoypadButton.Left, false);
                    break;
                case "s":
                    joypad.SetButtonPressedStatus(JoypadButton.Down, false);
                    break;
                case "d":
                    joypad.SetButtonPressedStatus(JoypadButton.Right, false);
                    break;
                case "v":
                    joypad.SetButtonPressedStatus(JoypadButton.Start, false);
                    break;
                case "b":
                    joypad.SetButtonPressedStatus(JoypadButton.Select, false);
                    break;
                case "j":
                    joypad.SetButtonPressedStatus(JoypadButton.ButtonA, false);
                    break;
                case "k":
                    joypad.SetButtonPressedStatus(JoypadButton.ButtonB, false);
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
                joypad = new();

                // ToDo: Initialize settings, load textures and sounds here
                cpu = new("nestest.nes", (ppu, o) =>
                {

                    var frame = new Frame();
                    Renderer.Render(ppu, frame);
                    screenState = frame.Data;
                }, joypad);

                cpu.Reset();

                //screenState = Array.Empty<byte>();
                //foreach (var tile in Enumerable.Repeat(0, 1).Select(x => Core.Frame.ShowFrame(cpu.Bus.ROM.CHRROM, 1, x).Data))
                //{
                //    screenState = screenState.Concat(tile).ToArray();
                //}
                //var tileFrame = Core.Frame.ShowTileBank(cpu.Bus.ROM.CHRROM, 0);
                //screenState = tileFrame.Data;
            };

            Game.Resize += (sender, e) =>
            {
                GL.Viewport(0, 0, Game.Size.Width, Game.Size.Height);
            };

            #region Initializating of static logic
            Color GetColor(byte data)
            {
                return data switch
                {
                    0 => Color.Black,
                    1 => Color.White,
                    2 or 9 => Color.DarkGray,
                    3 or 10 => Color.Red,
                    4 or 11 => Color.Green,
                    5 or 12 => Color.Blue,
                    6 or 13 => Color.Magenta,
                    7 or 14 => Color.Yellow,
                    _ => Color.Cyan,
                };
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var cpuTask = new Thread(() =>
            {
                try
                {
                    cpu.Run();
                }
                catch (Exception e)
                {

                }
            })
            {
                Name = "NESCPUExecution",
                Priority = ThreadPriority.Highest
            };

            cpuTask.Start();
        }
        #endregion
    }
}
