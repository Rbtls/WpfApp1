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

        public LinkedList<Connection> ConnectionsList = new LinkedList<Connection>();

        public NeuralNetwork()
        {
            // Network parameters
            Lambda = 20;  // insert frequency 
            Age_max = 15; // maximum connection age
            Alpha = 0.5f; // variable used for mistakes adaptation
            Eps_w = 0.028f; // Eps_w and Eps_n are used for weights adaptation
            Eps_n = 0.0006f;
            Max_nodes = 100; // maximum amount of nodes

            // Creating new connection
            NeursNum = 0;
            Connection _conn = new Connection(ref NeursNum);
            ConnectionsList.AddFirst(_conn);

            // Debug
            //////////////////////adding new neuron
            Connection _conn2 = new Connection(ref NeursNum, 2, ConnectionsList.Last.Value.ConnNeur2.Neur_X,
                ConnectionsList.Last.Value.ConnNeur2.Neur_Y);

            ConnectionsList.Last.Value.ConnNeur2.Synapses.Add(new NumWeights(1, _conn2.ConnNeur1.NeurNum, _conn2.ConnNeur1.Neur_X,
                _conn2.ConnNeur1.Neur_Y));

            ConnectionsList.AddLast(_conn2);
        }

        // set index in input vector (the space occupied by neuron)  //////////////////////////Work In Progress/////////
        public void SetInd(int ConnInd, int ind)
        {
            ConnectionsList.ElementAt(ConnInd).ConnNeur1.NeurIndInput = ind;
        }

        public long GetNNnum()
        {
            return NeursNum;
        }

        private Neuron FindNeuron(long uid)
        {
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.ConnNeur1.NeurNum == uid)
                {
                    return _conn.ConnNeur1;
                }
                else if (_conn.ConnNeur2.NeurNum == uid)
                {
                    return _conn.ConnNeur2;
                }
            }
            return null;
        }

        private void FindWinners() ////////////////Work In Progress////////////
        {
            if (Error_min == 0)
            {
                Error_min = ConnectionsList.ElementAt(0).ConnNeur1.Error;
                Winner = ConnectionsList.ElementAt(0).ConnNeur1;
                Secondwinner = Winner;
            }
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.ConnNeur1.Error < Error_min)
                {
                    Secondwinner = Winner;
                    Error_min = _conn.ConnNeur1.Error;
                    Winner = _conn.ConnNeur1;
                }
                else if (_conn.ConnNeur2 != null)
                {
                    if (_conn.ConnNeur2.Error < Error_min)
                    {
                        Secondwinner = Winner;
                        Error_min = _conn.ConnNeur2.Error;
                        Winner = _conn.ConnNeur2;
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
            /*Thread Th1 = new Thread(new ThreadStart(Winner.ProcessDistance));

            //setting the direction of search to forward
            Winner.Forward = true;

            //starting forward search in a separate thread in order to create a parallel search in both directions  
            Th1.Start();

            //starting backward search in the main thread
            Winner.ProcessDistance();*/

            Parallel.Invoke(
                () => Winner.ForwardSearch(),
                () => Winner.BackwardSearch()                
            );

            Winner.CompareDistances();

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
            if (FindNeuron(Id).Left == false)
            {
                // Calculate the increase value (check whether delta + x is out of Vpw borders)
                float IncRowsL = (FindNeuron(Id).Delta + FindNeuron(Id).Neur_X) / (MainWindow.Vpw - (2 * MainWindow._frame)); //should be vpw - borders!
                
                // When Delta is out of the X limit, increase Y
                if (IncRowsL > 1)
                {
                    // Increasing Y by the amount of rows            
                    FindNeuron(Id).Neur_Y += (int)IncRowsL / MainWindow._pixelSize;

                    // Reducing Delta value by the amount of rows for further increase of the X value
                    FindNeuron(Id).Neur_X += (FindNeuron(Id).Delta - (int)IncRowsL);

                    // Assigning new Index value due to the change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
                else
                {
                    // Increasing X value by the amount of Delta value
                    FindNeuron(Id).Neur_X += FindNeuron(Id).Delta;

                    // Assigning new Index value due to the change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
            }
            else // If neuron's position is to the right of the input array decrease coordinates' values by the amount of Delta value
            {
                // Calculate the increase value
                float IncRowsR = (FindNeuron(Id).Neur_X - FindNeuron(Id).Delta) / (MainWindow.Vpw - 2 * MainWindow._frame); //should be vpw - borders!

                if (IncRowsR < 0)
                {
                    // Decreasing Y by removing the integer part of Delta/X division (the amount of rows with width==X each)
                    FindNeuron(Id).Neur_Y -= (int)(FindNeuron(Id).Delta / FindNeuron(Id).Neur_X) / MainWindow._pixelSize; 

                    // Reducing Delta value by the amount of rows for further decrease of the X value
                    FindNeuron(Id).Neur_X -= (FindNeuron(Id).Delta - (FindNeuron(Id).Neur_Y * FindNeuron(Id).Neur_X));

                    // Assigning new Index value due to the change in coordinates
                    FindNeuron(Id).NeurIndInput = MainWindow.CalculateIndex(FindNeuron(Id).Neur_X, FindNeuron(Id).Neur_Y);

                    // Changing coordinates for visualisation
                    FindNeuron(Id).ChangePosition();
                }
                else
                {
                    // Decreasing X value by the amount of Delta value
                    FindNeuron(Id).Neur_X -= FindNeuron(Id).Delta;

                    // Assigning new Index value due to the change in coordinates
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
