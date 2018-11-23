using OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using Microsoft.Win32;


namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>


    public partial class MainWindow : System.Windows.Window
    {
        public NeuralNetwork network;

        //neuron's size in visualisation
        public const float K = 0.02f;
        
        //neuron's coordinates in visualisation
        public static float X_visual { get; set; } 
        public static float Y_visual { get; set; }

        public static List<OpenGL.Vertex3f> _Position { get; set; } = new List<OpenGL.Vertex3f> { };
        public static List<ushort> _TriangleEdges { get; set; } = new List<ushort> { };
        public static List<ushort> _ConnEdges { get; set; } = new List<ushort> { };
        public static List<float> _ArrayColor { get; set; } = new List<float> { };

        public static uint _TriangleVao { get; set; }
        public static uint _ConnEdgesBuffer { get; set; }
        public static uint _TriangleVerticesBuffer { get; set; }
        public static uint _TriangleEdgesBuffer { get; set; }
        public static uint _ColorBuffer { get; set; }

        public static Vertex3f[] _PositionArr { get; set; }
        public static ushort[] _ElementArr { get; set; }
        public static float[] _Color { get; set; }
        public static ushort[] _ConnElArr { get; set; }
        public static bool Change_pos { get; set; } = false;

        public static int debug1;
        public static int debug2;
        public static int debug3;
        public static int debug4;
        public static int debug5;
        public static int debug6;
        public static int debug7;
        public static int debug8;

        //image to open
        public static System.Drawing.Bitmap _image { get; set; }
        
        public static uint _Texture { get; set; }

        //Image ratio for resize purposes (newly opened image)
        public static float _Ratio { get; set; }

        //gl client's window related height (vph) & width (vpw)
        public static int Vpw { get; set; }
        public static int Vph { get; set; } 

        public static uint _tframebuffer { get; set; }

        //part of the screen that isn't filled with any content (only 1 out of 2 frames with the same width)
        public static float _frame { get; set; }

        public static float _pixelSize { get; set; }

        /*main input vector (the sequence of rgb values placed one after another, 
        _Input[4*ind] being the color value for the blue color, _Input[4*ind]+1 == green, _Input[4*ind]+2 == red)*/
        public static byte[] MainInput { get; set; } 


        public MainWindow()
        {
            InitializeComponent();
           
        }

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void GlControl_ContextCreated(object sender, OpenGL.GlControlEventArgs e)
        {
            Gl.ClearColor(0.0f, 1.0f, 1.0f, 1.0f);
            Gl.MatrixMode(MatrixMode.Projection);
            Gl.LoadIdentity();
            Gl.Ortho(0.0, 1.0f, 0.0, 1.0, 0.0, 1.0);

            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();
            CreateResources();
        }

        private void GlControl_Render(object sender, OpenGL.GlControlEventArgs e)
        {
            var senderControl = sender as GlControl;

            int vpx = 0;
            int vpy = 0;

            //actual width/height of render area
            Vpw = senderControl.ClientSize.Width;  
            Vph = senderControl.ClientSize.Height;

            Gl.Viewport(vpx, vpy, Vpw, Vph);

            if (Change_pos == true)
            {
                GlUpdateBuffers();
                Change_pos = false;
            }

            GlRender();
        }
        
        public static void GlRender()
        {
            if (MainWindow._image != null)
            {
                if (MainWindow._image.Width != 0)
                {
                    Gl.Enable(EnableCap.Texture2d);
                    Gl.BindTexture(TextureTarget.Texture2d, MainWindow._Texture);
                   
                    Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _tframebuffer);
                    Gl.Begin(PrimitiveType.Quads);
                    if (MainWindow._image.Height >= MainWindow._image.Width)
                    {
                        Gl.TexCoord2(0, 1);
                        Gl.Vertex3(((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw,
                            0, 0);

                        Gl.TexCoord2(1, 1);
                        Gl.Vertex3((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw,
                            0, 0);

                        Gl.TexCoord2(1, 0);
                        Gl.Vertex3((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw,
                            1, 0);

                        Gl.TexCoord2(0, 0);
                        Gl.Vertex3(((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) /2) / MainWindow.Vpw, 
                            1, 0);

                        _frame = (float)(((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2) * ((float)_image.Height / Vph));
                    }
                    else
                    {
                        Gl.TexCoord2(0, 1);
                        Gl.Vertex3(0,
                            ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph, 0);

                        Gl.TexCoord2(1, 1);
                        Gl.Vertex3(1,
                            ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph, 0);

                        Gl.TexCoord2(1, 0);
                        Gl.Vertex3(1,
                           (MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph, 0);

                        Gl.TexCoord2(0, 0);
                        Gl.Vertex3(0,
                           (MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph, 0);

                        _frame = (float)(((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) * ((float)_image.Width / Vpw));
                    }
                    Gl.End();
                    Gl.CopyTexSubImage2D(TextureTarget.Texture2d,
                    0, // level
                    0, 0, // offset
                    0, 0, // x, y
                    0, 0); // screenX, screenY
                    Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _tframebuffer);
                    Gl.Disable(EnableCap.Texture2d);
                }
            }

            _PositionArr = _Position.ToArray();
            _ElementArr = _TriangleEdges.ToArray();
            _Color = _ArrayColor.ToArray();
            _ConnElArr = _ConnEdges.ToArray();

            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._TriangleVerticesBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertex3f.Size *
                _PositionArr.Length), _PositionArr, BufferUsage.DynamicDraw);

            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._TriangleEdgesBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(2 * _ElementArr.Length),
                _ElementArr, BufferUsage.DynamicDraw);
            
            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._ColorBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * (_Color.Length)),
                _Color, BufferUsage.DynamicDraw);

            Gl.EnableClientState(EnableCap.ColorArray);
            Gl.ColorPointer(3, OpenGL.ColorPointerType.Float, 0, IntPtr.Zero);

            Gl.DrawElements(PrimitiveType.Triangles,
               _ElementArr.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _ConnEdgesBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(2 * _ConnElArr.Length),
                _ConnElArr, BufferUsage.DynamicDraw);

            Gl.DrawElements(PrimitiveType.Lines,
               _ConnElArr.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        } //-->GlRender()

        public static void GlUpdateBuffers()
        {
            /*_PositionArr = _Position.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._TriangleVerticesBuffer);
            Gl.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, (uint)(Vertex3f.Size * _PositionArr.Length), _PositionArr);

            _Color = _ArrayColor.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _ColorBuffer);
            Gl.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, (uint)(sizeof(float) * (_Color.Length)), _Color);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _ElementArr = _TriangleEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._TriangleEdgesBuffer);
            Gl.BufferSubData(BufferTarget.ElementArrayBuffer, System.IntPtr.Zero, (uint)(2 * _ElementArr.Length), _ElementArr);

            _ConnElArr = _ConnEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _ConnEdgesBuffer);
            Gl.BufferSubData(BufferTarget.ElementArrayBuffer, System.IntPtr.Zero, (uint)(2 * _ConnElArr.Length), _ConnElArr);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);*/
           /* Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);
            Gl.Clear(ClearBufferMask.StencilBufferBit);*/

            _PositionArr = _Position.ToArray();
            _ElementArr = _TriangleEdges.ToArray();
            _Color = _ArrayColor.ToArray();
            _ConnElArr = _ConnEdges.ToArray();

            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._TriangleVerticesBuffer);
            Gl.ClearBufferData(BufferTarget.ArrayBuffer, InternalFormat.Alpha12, PixelFormat.Alpha, PixelType.Bitmap, _PositionArr);
            //Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertex3f.Size * _PositionArr.Length), null, BufferUsage.DynamicDraw);
            //Gl.InvalidateBufferData(MainWindow._TriangleVerticesBuffer);
            //Gl.MapBufferRange(BufferTarget.ArrayBuffer, System.IntPtr.Zero, (uint)(Vertex3f.Size * _PositionArr.Length), );
           // Gl.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, (uint)(Vertex3f.Size * _PositionArr.Length), _PositionArr);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._TriangleEdgesBuffer);
            Gl.ClearBufferData(BufferTarget.ArrayBuffer, InternalFormat.Alpha12, PixelFormat.Alpha, PixelType.Bitmap, _ElementArr);
            //Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(2 * _ElementArr.Length), null, BufferUsage.DynamicDraw);
            //Gl.InvalidateBufferData(MainWindow._TriangleEdgesBuffer);
            // Gl.BufferSubData(BufferTarget.ElementArrayBuffer, System.IntPtr.Zero, (uint)(2 * _ElementArr.Length), _ElementArr);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._ColorBuffer);
            Gl.ClearBufferData(BufferTarget.ArrayBuffer, InternalFormat.Alpha12, PixelFormat.Alpha, PixelType.Bitmap, _Color);
            //Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * (_Color.Length)), null, BufferUsage.DynamicDraw);
            //Gl.InvalidateBufferData(MainWindow._ColorBuffer);
            //  Gl.BufferSubData(BufferTarget.ArrayBuffer, System.IntPtr.Zero, (uint)(sizeof(float) * (_Color.Length)), _Color);

            //Gl.DrawElements(PrimitiveType.Triangles, _ElementArr.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._ConnEdgesBuffer);
            Gl.ClearBufferData(BufferTarget.ArrayBuffer, InternalFormat.Alpha12, PixelFormat.Alpha, PixelType.Bitmap, _ConnElArr);
            //Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(2 * _ConnElArr.Length), null, BufferUsage.DynamicDraw);
            //Gl.InvalidateBufferData(MainWindow._ConnEdgesBuffer);
            //  Gl.BufferSubData(BufferTarget.ElementArrayBuffer, System.IntPtr.Zero, (uint)(2 * _ConnElArr.Length), _ConnElArr);

            //Gl.DrawElements(PrimitiveType.Lines, _ConnElArr.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);

            /*_PositionArr = _Position.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._TriangleVerticesBuffer);
            //Gl.InvalidateBufferData(MainWindow._TriangleVerticesBuffer);
            
            _ElementArr = _TriangleEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._TriangleEdgesBuffer);
            //Gl.InvalidateBufferData(MainWindow._TriangleEdgesBuffer);


            _Color = _ArrayColor.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _ColorBuffer);
            //Gl.InvalidateBufferData(MainWindow._ColorBuffer);


            _ConnElArr = _ConnEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _ConnEdgesBuffer);
            //Gl.InvalidateBufferData(MainWindow._ConnEdgesBuffer);*/


        } //-->GlUpdateBuffers()

        public static uint GenTexture()
        {
            uint _Tex = new uint();
            Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);

            Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, _Tex);

            System.Drawing.Imaging.BitmapData _BitmapData = MainWindow._image.LockBits(new System.Drawing.Rectangle(0, 0, _image.Width, _image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, _BitmapData.Width, _BitmapData.Height, 0,
                OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _BitmapData.Scan0);
           
            MainWindow._image.UnlockBits(_BitmapData);

            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return _Tex;
        } // -->GenTexture()

        private static void CreateResources()
        {
            CreateTriangleVertexArray();
        }

        public static void CreateTriangleVertexArray()
        {
            _TriangleVao = Gl.GenVertexArray();
            Gl.BindVertexArray(_TriangleVao);
            MainWindow._Position.Add(new OpenGL.Vertex3f(0.0f, 0.0f, 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(0.0f, 0.0f, 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(0.0f, 0.0f, 0.0f));
            MainWindow._TriangleEdges.Add((ushort)0);
            MainWindow._TriangleEdges.Add((ushort)1);
            MainWindow._TriangleEdges.Add((ushort)2);
            MainWindow._ConnEdges.Add((ushort)0);
            MainWindow._ConnEdges.Add((ushort)1);

            for (int i=0; i<9; i++)
            {
                MainWindow._ArrayColor.Add(0.0f);
            }
            _TriangleVerticesBuffer = Gl.GenBuffer();
            _TriangleEdgesBuffer = Gl.GenBuffer();
            _ColorBuffer = Gl.GenBuffer();
            _ConnEdgesBuffer = Gl.GenBuffer();

           // GlRender();
        } //-->CreateTriangleVertexArray()

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Train(object sender, RoutedEventArgs e)
        {
            //example on how to get the input values (but instead of reading image we have numerical values)
            /* var dataset = File.ReadAllLines(@"C:\Temp\2.csv");
            //------------------> 
            var allInputs = dataset.Select(x => x.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            TextBox1.Text += $"Training network with {allInputs.Length} samples..." + "\r\n";

            var normalizedInputs = allInputs.Select(x => new
            {
                Answer = x[0],
                Inputs = NormalizeInput(x.Skip(1).ToArray())
            }).ToArray(); */

            System.Drawing.Imaging.BitmapData _BtData = MainWindow._image.LockBits(new System.Drawing.Rectangle(0, 0, MainWindow._image.Width, MainWindow._image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr t = _BtData.Scan0;
            Gl.ReadPixels(0, 0, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, t);
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Declare an array to hold the bytes of the bitmap. This is also the input vector of NN.
            int bytes = Math.Abs(_BtData.Stride) * _image.Height;
            MainInput = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(t, MainInput, 0, bytes);

            network = new NeuralNetwork();

            /* // Commented due to occasional threading exception
            // Calculating neuron's index value based on neuron's X and Y coordinates used in visualisation
            int ind = CalculateIndex(X_visual, Y_visual);

            // Setting index value for the newly added neuron
            network.SetInd((network._connections.Count - 1) ,ind);

            // Debug info to show rgb values of the last added neuron
            int blue = (int)MainInput[4*ind];
            int green = (int)MainInput[(4*ind)+1];
            int red = (int)MainInput[(4*ind)+2]; */

            MainWindow._image.UnlockBits(_BtData);

            TextBox1.Text += $"Number of neurons = {network.GetNNnum()} " + "\r\n";
            TextBox1.Text += $"Length of bytes = {MainInput.Length} " + "\r\n" + "\r\n";
            TextBox1.Text += $"pixel#1 X = { _PositionArr[3].x} " + "\r\n";
            TextBox1.Text += $"pixel#1 Y = { _PositionArr[3].y} " + "\r\n";
            TextBox1.Text += $"pixel#2 X = { _PositionArr[6].x} " + "\r\n";
            TextBox1.Text += $"pixel#2 Y = { _PositionArr[6].y} " + "\r\n";
            TextBox1.Text += $"pixel#3 X = { _PositionArr[9].x} " + "\r\n";
            TextBox1.Text += $"pixel#3 Y = { _PositionArr[9].y} " + "\r\n";
            TextBox1.Text += $"debug1 = {debug1} " + "\r\n";
            TextBox1.Text += $"debug2 = {debug2} " + "\r\n";
            TextBox1.Text += $"debug3 = {debug3} " + "\r\n";
            TextBox1.Text += $"debug index = {debug4} " + "\r\n";
            //TextBox1.Text += $"pixel#2 X = {MainWindow.X_visual} " + "\r\n";
            //TextBox1.Text += $"pixel#2 Y = {MainWindow.Y_visual} " + "\r\n";
            /* TextBox1.Text += $"#2 pixel red = {red} " + "\r\n";
            TextBox1.Text += $"#2 pixel green = {green} " + "\r\n";
            TextBox1.Text += $"#2 pixel blue = {blue} " + "\r\n"; */

            //TextBox1.Text += $"winner.id = {network.Winner.NeurNum}" + "\r\n";
            //TextBox1.Text += $"winner.error = {network.Winner.Error}" + "\r\n";
            TestButton.IsEnabled = true;
        } //-->Train()

        private void Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog _OpenFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image files (*.jpeg;*.jpg;*.bmp;*.png)|*.jpeg;*.jpg;*.bmp;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (_OpenFileDialog.ShowDialog() == true)
            {
                TextBox1.Text += $"You have opened the {Path.GetFileName(_OpenFileDialog.FileName)} file" + "\r\n";
                MainWindow._image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(_OpenFileDialog.FileName);
                if (MainWindow._image.Height >= MainWindow._image.Width)
                {
                    MainWindow._Ratio = ((float)MainWindow._image.Width / (float)MainWindow._image.Height);
                    _pixelSize = (float)Vph / _image.Height;
                }
                else
                {
                    MainWindow._Ratio = ((float)MainWindow._image.Height / (float)MainWindow._image.Width);
                    _pixelSize = (float)Vpw / _image.Width;
                }
                TextBox1.Text += $"Image resolution = {MainWindow._image.Width}" + $"x{MainWindow._image.Height} pixels" + "\r\n";
                MainWindow._Texture = GenTexture();
                TrainButton.IsEnabled = true;
            }
        } //-->Open()

        private static double[] NormalizeInput(string[] input)
        {
            return input
                .Select(double.Parse)
                .Select(y => (y / 255) * 0.99 + 0.01)
                .ToArray();
        }

        public static int CalculateIndex(float X, float Y)
        {
            int ind;
            //Current neuron's index to look for in main input array MainInput ("input vector x").
            if (Y == 0)
            {
                ind = (int)(((_image.Width) * (int)(_image.Height * (1.0 - Y))) - (int)(_image.Width - (_image.Width * X)));
                return ind;

            }
            else
            {
                ind = (int)((_image.Width * X + 0.5 * K) + ((_image.Width) * (int)(_image.Height * (1.0 - Y)) - _image.Width));
                //ind = (int)(((vph * X)/_pixelSize) + (((((1.0 - Y) * vph) - 1) / _pixelSize)* (vph / _pixelSize)) );
                return ind;
            }
        }

        private void Testbtn(object sender, RoutedEventArgs e)
        {
            network.ProcessVector();
            //TextBox1.Text += $"pixel#2 X = {MainWindow.X_visual} " + "\r\n";
            //TextBox1.Text += $"pixel#2 Y = {MainWindow.Y_visual} " + "\r\n";
            TextBox1.Text += "\r\n";
            TextBox1.Text += $"pixel#1 X = { _PositionArr[3].x} " + "\r\n";
            TextBox1.Text += $"pixel#1 Y = { _PositionArr[3].y} " + "\r\n";
            TextBox1.Text += $"pixel#2 X = { _PositionArr[6].x} " + "\r\n";
            TextBox1.Text += $"pixel#2 Y = { _PositionArr[6].y} " + "\r\n";
            TextBox1.Text += $"pixel#3 X = { _PositionArr[9].x} " + "\r\n";
            TextBox1.Text += $"pixel#3 Y = { _PositionArr[9].y} " + "\r\n";
            TextBox1.Text += $"debug5 = {debug5} " + "\r\n";
            TextBox1.Text += $"debug6 = {debug6} " + "\r\n";
            TextBox1.Text += $"debug7 = {debug7} " + "\r\n";
            TextBox1.Text += $"debug index = {debug8} " + "\r\n";
        }
    } //-->MainWindow
       
}