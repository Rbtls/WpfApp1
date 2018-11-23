using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class NeuralNetwork
    {
        //Main number of neurons
        private long NeursNum;
        public int Error_min { get; set; }
        public Neuron Winner { get; set; }
        public Neuron Secondwinner { get; set; }
        public int Iteration_number { get; set; }

        public int Max_nodes { get; set; }
        public int Lambda { get; set; }
        public int Age_max { get; set; }
        //alpha & beta are used to adapt errors
        public float Alpha { get; set; }
        public float Beta { get; set; }                         //////////////////////
        //eps_w & eps_n are used to adapt weights
        public static float Eps_w { get; set; }
        public static float Eps_n { get; set; }

        public LinkedList<Connection> _connections = new LinkedList<Connection>();

        public NeuralNetwork()
        {
            // Creating new connection
            NeursNum = 0;
            Connection _conn = new Connection(ref NeursNum);
            _connections.AddFirst(_conn);

            // Debug
            //////////////////////adding new neuron
            Connection _conn2 = new Connection(ref NeursNum, 2, _connections.Last.Value._neur2.Neur_X,
                _connections.Last.Value._neur2.Neur_Y);

            _connections.Last.Value._neur2.Synapses.Add(new NumWeights(1, _conn2._neur1.NeurNum, _conn2._neur1.Neur_X,
                _conn2._neur1.Neur_Y));

            _connections.AddLast(_conn2);

            Lambda = 20;
            Age_max = 15;
            Alpha = 0.5f;
            Eps_w = 0.05f;
            Eps_n = 0.0006f;
            Max_nodes = 100;
        }

        //set index in input vector (the space occupied by neuron)  //////////////////////////Work In Progress/////////
        public void SetInd(int ConnInd, int ind)
        {
            _connections.ElementAt(ConnInd)._neur1.NeurIndInput = ind;
        }

        public long GetNNnum()
        {
            return NeursNum;
        }

        private Neuron FindNeuron(long uid)
        {
            foreach (Connection _conn in _connections)
            {
                if (_conn._neur1.NeurNum == uid)
                {
                    return _conn._neur1;
                }
                else if (_conn._neur2.NeurNum == uid)
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

            //2. Searching for the nearest value in _Input vector (this way we can calculate the distance between the node and the value)
            Thread Th1 = new Thread(new ThreadStart(Winner.ProcessDistance));

            //setting the direction of search to forward
            Winner.Forward = true;

            //starting forward search in a separate thread in order to create a parallel search execution in both directions  
            Th1.Start();

            //changing direction of search to backwards
            Winner.Forward = false;

            //starting backward search in the main thread
            Winner.ProcessDistance();

            //3. Changing winner's local error
            Winner.E += Winner.Error;

            //4. Move winner and all of it's topological neighbours towards _Input vector using Delta value (calculated with Eps_w)
            Adapt_weights(Winner.NeurNum);

            //5. 

        }

        // Change neuron's and it's neighbours' positions  /////////////////////////////////WIP
        public void Adapt_weights(long Id/*id of neuron*/)
        {
            // If neuron's position is to the left of the input array increase coordinates' values by the amount of Delta value
            if (FindNeuron(Id).Left == true) //neuron's X&Y is < than input
            {
                // Calculate the increase value
                float IncValL = (FindNeuron(Id).Delta + FindNeuron(Id).Neur_X) / MainWindow.Vpw;

                // When Delta is out of the X limit, increase Y...
                if (IncValL > 1)
                {
                    // Increasing Y by the amount of rows
                    FindNeuron(Id).Neur_Y += (int)IncValL - 1;

                    // Reducing Delta value by the amount of rows for further increase of X value
                    FindNeuron(Id).Neur_X += FindNeuron(Id).Delta - MainWindow.Vpw * IncValL;

                    // Assigning new Index value due to the change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
                else
                {
                    // Increasing X value by the amount of Delta value
                    FindNeuron(Id).Neur_X += FindNeuron(Id).Delta;

                    // Assigning new Index value due to change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
            }
            else // If neuron's position is to the right of the input array reduce coordinates' values by the amount of Delta value
            {
                // Calculate the increase value
                float IncValR = (FindNeuron(Id).Neur_X - FindNeuron(Id).Delta) / MainWindow.Vpw;

                if (IncValR < 0)
                {
                    // Decreasing Y by the amount of rows
                    FindNeuron(Id).Neur_Y -= (int)(FindNeuron(Id).Delta / FindNeuron(Id).Neur_X);

                    // Reducing Delta value by the amount of rows for further decrease of X value
                    FindNeuron(Id).Neur_X -= (FindNeuron(Id).Delta - (FindNeuron(Id).Neur_Y * FindNeuron(Id).Neur_X));

                    // Assigning new Index value due to change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
                else
                {
                    // Decreasing X value by the amount of Delta value
                    FindNeuron(Id).Neur_X -= FindNeuron(Id).Delta;

                    // Assigning new Index value due to change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
            }

            //System.Threading.Thread.Sleep(1000);


            //////////////////////////////////////////////////////WIP
            // Find neighbours' axons/synapses values related to parent neuron for further weight change
            // Change neighbours positions
        }

    } //-->NeuralNetwork
}
