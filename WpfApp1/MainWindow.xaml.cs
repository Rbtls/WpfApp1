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
            Vpw = senderControl.ClientSize.Width;  //actual width/height of render area
            Vph = senderControl.ClientSize.Height;

            Gl.Viewport(vpx, vpy, Vpw, Vph);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.BindVertexArray(_TriangleVao);

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
            
            Vertex3f[] _PositionArr = _Position.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, MainWindow._TriangleVerticesBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertex3f.Size *
            _PositionArr.Length), _PositionArr, BufferUsage.DynamicDraw);

            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);

            ushort[] _ElementArr = _TriangleEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, MainWindow._TriangleEdgesBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer,
                (uint)(2 * _ElementArr.Length), _ElementArr,
                BufferUsage.DynamicDraw);

            float[] _Color = _ArrayColor.ToArray();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _ColorBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * (_Color.Length)),
                _Color, BufferUsage.DynamicDraw);

            Gl.EnableClientState(EnableCap.ColorArray);
            Gl.ColorPointer(3, OpenGL.ColorPointerType.Float, 0, IntPtr.Zero);
           
            Gl.DrawElements(PrimitiveType.Triangles,
               _ElementArr.Length,
               DrawElementsType.UnsignedShort,
               IntPtr.Zero
             );

            ushort[] _ConnElArr = _ConnEdges.ToArray();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _ConnEdgesBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer,
                (uint)(2 * _ConnElArr.Length), _ConnElArr, BufferUsage.DynamicDraw);

            Gl.DrawElements(PrimitiveType.Lines,
               _ConnElArr.Length,
               DrawElementsType.UnsignedShort,
               IntPtr.Zero
             );
            
        }

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
        }

        private static void CreateResources()
        {
            CreateTriangleVertexArray();
        }

        private static void CreateTriangleVertexArray()
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
            GlRender();
        }

        public const float K = 0.02f; //neuron's size in visualisation
        public static float X_visual { get; set; } //neuron's coordinates in visualisation
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
        public static System.Drawing.Bitmap _image { get; set; } //opened image
        public static uint _Texture { get; set; }
        public static float _Ratio { get; set; } //Image ratio for resize purposes (newly opened image)
        public static int Vpw { get; set; }
        public static int Vph { get; set; } //gl client's window related height (vph) & width (vpw)
        public static uint _tframebuffer { get; set; }
        public static float _frame { get; set; } //part of the screen that isn't filled with any content (only 1 out of 2 frames with the same width)
        public static float _pixelSize { get; set; }
        public static byte[] MainInput { get; set; } //main input vector (the sequence of rgb values placed one after another, _Input[4*ind] being the color value of blue color, _Input[4*ind]+1 — green, _Input[4*ind]+2 — red)

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Train(object sender, RoutedEventArgs e)
        {
            var network = new NeuralNetwork();

            //different example of how to get input values but instead of reading image here we have numerical values
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

            // Calculating neuron's index value based on neuron's X and Y coordinates used in visualisation
            int ind = CalculateIndex(X_visual, Y_visual);

            // Setting index value for the newly added neuron
            network.SetInd((network._connections.Count - 1) ,ind);

            // Debug info to show rgb values of the last added neuron
            int blue = (int)MainInput[4*ind];
            int green = (int)MainInput[(4*ind)+1];
            int red = (int)MainInput[(4*ind)+2];

            MainWindow._image.UnlockBits(_BtData);
            
            TextBox1.Text += $"Number of neurons = {network.GetNNnum()} " + "\r\n";
            TextBox1.Text += $"Length of bytes = {MainInput.Length} " + "\r\n";
            TextBox1.Text += $"index = {ind} " + "\r\n";
            TextBox1.Text += $"pixel#2 X = {MainWindow.X_visual} " + "\r\n";
            TextBox1.Text += $"pixel#2 Y = {MainWindow.Y_visual} " + "\r\n";
            TextBox1.Text += $"#2 pixel red = {red} " + "\r\n";
            TextBox1.Text += $"#2 pixel green = {green} " + "\r\n";
            TextBox1.Text += $"#2 pixel blue = {blue} " + "\r\n";

            network.ProcessVector();
            TextBox1.Text += $"winner.id = {network.Winner.NeurNum}" + "\r\n";
            TextBox1.Text += $"winner.error = {network.Winner.Error}" + "\r\n";
        }

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
        }

        private static double[] NormalizeInput(string[] input)
        {
            return input
                .Select(double.Parse)
                .Select(y => (y / 255) * 0.99 + 0.01)
                .ToArray();
        }

        public int CalculateIndex(float X, float Y)
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
    } //-->MainWindow

    public class NeuralNetwork
    {
        //Main number of neurons
        private long NeursNum;
        public int Error_min { get; set; }
        public Neuron Winner { get; set; }
        public Neuron Secondwinner { get; set; }
        public int Iteration_number {get; set;}

        public int Max_nodes { get; set; }
        public int Lambda { get; set; }
        public int Age_max { get; set; }
        //alpha & beta are used to adapt errors
        public double Alpha { get; set; }
        public double Beta { get; set; }                         //////////////////////
        //eps_w & eps_n are used to adapt weights
        public static double Eps_w { get; set; }
        public static double Eps_n { get; set; }

        public LinkedList<Connection> _connections = new LinkedList<Connection>();

        public NeuralNetwork()
        {
            // Creating new connection
            NeursNum = 0;
            Connection _conn = new Connection(ref NeursNum);
            _connections.AddFirst(_conn);

            // Debug
            Connection _conn2 = new Connection(ref NeursNum, 2, _connections.Last.Value._neur2.Neur_X, _connections.Last.Value._neur2.Neur_Y);//////////////////////adding new neuron...
            _connections.Last.Value._neur2.Synapses.Add(new NumWeights(1, _conn2._neur1.Neur_X, _conn2._neur1.Neur_Y));
            _connections.AddLast(_conn2);
         
            Lambda = 20;
            Age_max = 15;
            Alpha = 0.5;
            Eps_w = 0.05;
            Eps_n = 0.0006;
            Max_nodes = 100;
        }

        //set index in input vector (space occupied by the neuron)  //////////////////////////Work In Progress/////////
        public void SetInd(int ConnInd, int ind)
        {
          _connections.ElementAt(ConnInd)._neur1.NeurInInput = ind;
        }

        public long GetNNnum()
        {
            return NeursNum;
        }

        private Neuron FindNeuron(long uid)
        {
            foreach(Connection _conn in _connections)
            {
                if (_conn._neur1.NeurNum == uid)
                {
                    return _conn._neur1;
                }
                else if(_conn._neur2.NeurNum == uid)
                {
                    return _conn._neur2;
                }
            }
            return null;
        }

        private void FindWinners() ////////////////Work In Progress////////////
        {
            if (Error_min == 0)
            {
                Error_min = _connections.ElementAt(0)._neur1.Error;
                Winner = _connections.ElementAt(0)._neur1;
                Secondwinner = Winner;
            } 
            foreach (Connection _conn in _connections)
            {
                if (_conn._neur1.Error < Error_min)
                {
                    Secondwinner = Winner;
                    Error_min = _conn._neur1.Error;
                    Winner = _conn._neur1;
                }
                else if (_conn._neur2 != null)
                {
                    if (_conn._neur2.Error < Error_min)
                    {
                        Secondwinner = Winner;
                        Error_min = _conn._neur2.Error;
                        Winner = _conn._neur2;
                    }
                }
            }
        }

        public void ProcessVector()
        {
            //0. Increase iteration number
            Iteration_number++;

            //1.
            FindWinners();

            //2. Searching for nearest value in _Input vector (this way we can calculate the distance between the node and the value)
            Thread Th1 = new Thread(new ThreadStart(Winner.ProcessDistance));
            //setting direction of search to forward
            Winner.Forward = true;
            //starting forward search in separate thread to make parallel execution of search in both directions  
            Th1.Start();
            //changing direction of search to backwards
            Winner.Forward = false;
            //starting backward search in main thread
            Winner.ProcessDistance();

            //3. Changing winner's local error
            Winner.E += Winner.Error;

            //4. Move winner and all of it's topological neighbours towards _Input vector using Delta value (calculated with Eps_w)
            Adapt_weights(Winner.NeurNum);

            //5. 
        }

        //change neuron's and it's neighbours' positions  /////////////////////////////////WIP
        public void Adapt_weights(long Id/*id of neuron*/)
        { 
            //if neuron's position is from the left of the input array
           /* if ()//neuron's X&Y is < than input
            {
              //neuron += delta
              //
            }
            else
            {

            }*/
        }

    } //-->NeuralNetwork

    public class Connection
    {
        public Neuron _neur1 { get; set; }
        public Neuron _neur2 { get; set; }
        public float _weight { get; set; }
        public int Age { get; set; }
        public long _id1 { get; set; } //ids of nodes that should be connected 
        public long _id2 { get; set; }
        private static Random rnd = new Random();

        //connection between first two neurons when we don't have the id of the parent neuron
        public Connection(ref long num)
        {
            Age = 0;
            var TempX_1 = (float)0.1 * (rnd.Next(1, 10));
            var TempY_1 = (float)0.1 * (rnd.Next(1, 10));
            _neur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id1 = _neur1.NeurNum;

            var TempX_2 = (float)0.1 * (rnd.Next(0, 9));
            var TempY_2 = (float)0.1 * (rnd.Next(0, 9));
            _neur2 = new Neuron(++num, TempX_2, TempY_2);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id2 = _neur2.NeurNum;
            
            //adding output info to neur1
            _neur1.Axons.Add(new NumWeights(1, TempX_2, TempY_2));
            //adding input info to neur2
            _neur2.Synapses.Add(new NumWeights(1, TempX_1, TempY_1));

            MainWindow.GlRender();
        }

        // Creating connection when parent neuron already exists
        public Connection(ref long num, long previous_id, float X_parent, float Y_parent) 
        {
            Age = 0;
            var TempX_1 = (float)0.1 * (rnd.Next(1, 10));
            var TempY_1 = (float)0.1 * (rnd.Next(1, 10));
            _neur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id1 = _neur1.NeurNum;
            
            //adding output info to _neur1.Axons array, Axons.Count being new id for the new weight that's being added
            _neur1.Axons.Add(new NumWeights(_neur1.Axons.Count, X_parent, Y_parent));
                        
            //send info (about what neuron should be connected to _neur1) to the list of connections in the opengl mode
            MainWindow._ConnEdges.Add((ushort)(previous_id + (2 * previous_id)));
            _id2 = previous_id;
            
            MainWindow.GlRender();
        }
    } //-->Connection

    public class Neuron
    {
        //neuron's number
        public long NeurNum { get; private set; }

        //index in input vector (index of pixel occupied by the neuron)
        public int NeurInInput { get; set; }

        //neuron's position
        public float Neur_X { get; private set; }
        public float Neur_Y { get; private set; }

        //distance to move node after processing 
        public double Delta { get; set; }

       /* //output axon connection to the id
        public int Next_num { get => next_num; set => next_num = value; }

        //output axon connection weight
        public float Next_weight { get => next_weight; set => next_weight = value; }*/

        //output axons
        public List<NumWeights> Axons { get; set; } = new List<NumWeights>();

        //synapses (inputs)
        public List<NumWeights> Synapses { get; set; } = new List<NumWeights>();

        //previous experience
        public List<PrevExp> Experience { get; set; } = new List<PrevExp>();

        //error
        public int Error { get; set; }
        
        //local error (sum of errors) 
        public int E { get; set; }

        //bool value representing whether distance has been processed or not
        bool Processed { get; set; }
       
        //check direction of search for ProcessDistance method
        public bool Forward { get; set; }

        private static Random _rnd = new Random();
        
        public Neuron(long num)
        {
            NeurNum = num;
            Error = 0;
            NeurInInput = -1;
            Processed = false;
        }

        public Neuron(long num, float _x, float _y)
        {
            NeurNum = num;
            Error = 0;
            NeurInInput = -1;
            Processed = false;

            //making limit for neuron's position so it wouldn't be possible for it to get out of the picture's borders
            if (((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) < _x)
            {
                _x = ((MainWindow.Vpw - ((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2)) / MainWindow.Vpw) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.K);
            }

            if ((((MainWindow.Vpw - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw) > _x)
            {
                _x = (((MainWindow.Vpw - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vph)) / 2) / MainWindow.Vpw);
            }

            if (((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) < _y)
            {
                _y = ((MainWindow.Vph - ((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2)) / MainWindow.Vph) - ((float)1 * (_rnd.Next(0, 9))) - (1.0f * MainWindow.K);
            }

            if ((((MainWindow.Vph - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph) > _y)
            {
                _y = (((MainWindow.Vph - ((float)1 * (_rnd.Next(0, 9))) - (MainWindow._Ratio * MainWindow.Vpw)) / 2) / MainWindow.Vph);
            }

            MainWindow.X_visual = Neur_X = _x;
            MainWindow.Y_visual = Neur_Y = _y;
            
            //drawing triangle representing one node:
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.0f * MainWindow.K) + MainWindow.X_visual),
               ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((0.5f * MainWindow.K) + MainWindow.X_visual),
             ((1.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));
            MainWindow._Position.Add(new OpenGL.Vertex3f(((1.0f * MainWindow.K) + MainWindow.X_visual),
             ((0.0f * MainWindow.K) + MainWindow.Y_visual), 0.0f));

            MainWindow._TriangleEdges.Add((ushort)(num+(2*num)+0));
            MainWindow._TriangleEdges.Add((ushort)(num+(2*num)+1));
            MainWindow._TriangleEdges.Add((ushort)(num+(2*num)+2));

            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(1.0f);
            MainWindow._ArrayColor.Add(0.0f);
            MainWindow._ArrayColor.Add(1.0f);

        }

        ~Neuron() { }

        //neuron's comparison algorithm       ////////////////WIP
        private void Compare()
        {

        }

        //for first winner find the nearest value in MainInput vector and update local error 
        public void ProcessDistance() /////////////////////////////////Work In Progress////////////
        {
            //check direction of search
            if (Forward == true)
            {
                for (int i = NeurInInput; i < MainWindow.MainInput.Length; i++)
                {
                    //searching for the first pixel that is different from black background
                    if ((Processed == false) && ((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[4 * i + 1] > 0) || (MainWindow.MainInput[4 * i + 2] > 0)))
                    {
                        Error += (int)Math.Pow(Math.Abs((Neur_X + ((i - NeurInInput) * MainWindow._pixelSize)) - Neur_X), 2); 
                        //changing Delta (distance) for further change of winner's position and it's synapses values and neighbours' position and their axons values accordingly
                        Delta = NeuralNetwork.Eps_w * ((i - NeurInInput) * MainWindow._pixelSize); 
                        Processed = true;
                        break;
                    }
                    Thread.Sleep(0);
                }
            }
            else
            {
                for (int i = NeurInInput; i >= 2; i--)
                {
                    if ((Processed == false) && ((MainWindow.MainInput[4 * i] > 0) || (MainWindow.MainInput[4 * i - 1] > 0) || (MainWindow.MainInput[4 * i - 2] > 0)))
                    {
                        Error += (int)Math.Pow(Math.Abs((Neur_X + ((NeurInInput - i) * MainWindow._pixelSize)) - Neur_X), 2);
                        Delta = NeuralNetwork.Eps_w * ((NeurInInput - i) * MainWindow._pixelSize);
                        Processed = true;
                        break;
                    }
                    Thread.Sleep(0);
                }
            }
        }

    } //-->Neuron
    
    public struct NumWeights //neuron's connections (synapses, axons) id_of_weight/weight_amount(X,Y)
    {
        public NumWeights(int weight_id, float weightX, float weightY) 
        {
            WeightDataId = weight_id;
            WeightDataX = weightX;
            WeightDataY = weightY;
        }

        //input's weight id that is used in the linked list of 'weight ids' (starting from strongest weight to the weakest) 
        public int WeightDataId { get; private set; }         /////////////////////////////

        //the amount of weight of each synapse
        public float WeightDataX { get; set; }
        public float WeightDataY { get; set; }
    }

    public struct PrevExp //neuron's experience                  //~~~~~~~~~~~~?
    {
        public PrevExp(int x, int y)
        {
            Row = x;
            Column = y;
        }

        //Previous experience matrix coordinates x
        public int Row { get; private set; }

        //Previous experience matrix coordinates y
        public int Column { get; private set; } 
    }

}
