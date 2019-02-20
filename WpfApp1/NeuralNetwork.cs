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
        private long NeursCount;
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
            Eps_w = 0.028f; // Eps_w and Eps_n are used for weights adaptation (the distance between the node and the main input vector)
            Eps_n = 0.0006f;
            Max_nodes = 100; // maximum amount of nodes

            // Creating new connection
            NeursCount = 0;
            Connection _conn = new Connection(ref NeursCount);
            ConnectionsList.AddFirst(_conn);

            // Debug
            //////////////////////adding new neuron
            // adding new connection info (input) to the existing parent neuron (first parameter)
            Connection _conn2 = new Connection(ref ConnectionsList.Last.Value.ConnNeur2, _conn.ConnId, ref NeursCount);

            // adding new connection to the ConnectionsList
            ConnectionsList.AddLast(_conn2);

        }

        // set index in input vector (the space occupied by neuron)  //////WIP/////////
        public void SetInd(int ConnInd, int ind)
        {
            ConnectionsList.ElementAt(ConnInd).ConnNeur1.NeurIndInput = ind;
        }

        // get number of neurons in the network
        public long GetNetworkNeurCount()
        {
            return NeursCount;
        }

        private Neuron FindNeuron(long uid)
        {
            foreach (Connection _conn in ConnectionsList)
            {
                if (_conn.ConnNeur1.NeurId == uid)
                {
                    return _conn.ConnNeur1;
                }
                else if (_conn.ConnNeur2.NeurId == uid)
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

            //2. Searching for the nearest value in the _Input vector (this way we can calculate the distance between the node and the value)

            Parallel.Invoke(
                () => Winner.ForwardSearch(),
                () => Winner.BackwardSearch()
            );

            Winner.CompareDistances();

            //3. Changing winner's local error
            Winner.E += Winner.Error;

            //4. Move the winner and all of it's topological neighbours towards _Input vector using Delta value (calculated with Eps_w)
            Adapt_weights(Winner.NeurId, true);
            MoveNeighbours(Winner.NeurId);

            //5. Increase age of all connections coming out from the winner (axons) by 1 
            for (int i = 0; i < Winner.AxonsWeights.Count; i++)
            {
                ConnectionsList.ElementAt(Winner.AxonsWeights[i].ConnId).ConnAge += 1;
            }

            //6. If there is a connection between the first winner and the second winner change it's age to 0, else - create a connection between them

        }

        // Change neuron's and it's neighbours' positions 
        public void Adapt_weights(long parent_id, bool isWinner)
        {
            Neuron ParentNeur;

            // Reference to the parent neuron in the connection
            if (FindNeuron(parent_id) != null)
            {
                ParentNeur = FindNeuron(parent_id);
            }
            else
            {
                return;
            }

            // Check whether the node is the winner node (Eps_w is for the winner, Eps_n - for the neighbours)
            if (isWinner == false)
            {
                ParentNeur.Delta /= Eps_w;
                ParentNeur.Delta *= Eps_n;
            }

            // If neuron's position is to the left of the input array increase coordinates' values by the amount of Delta value
            if (ParentNeur.Left == false)
            {
                // Calculate the increase value (check whether delta + x is out of Vpw borders)
                float IncRowsL = (ParentNeur.Delta + ParentNeur.Neur_X) / (MainWindow.Vpw - (2 * MainWindow._frame)); //should be vpw - borders!

                // When Delta is out of the X limit, increase Y
                if (IncRowsL > 1)
                {
                    // Increasing Y by the amount of rows            
                    ParentNeur.Neur_Y += (int)IncRowsL / MainWindow._pixelSize;

                    // Reducing Delta value by the amount of rows for further increase of the X value
                    ParentNeur.Neur_X += (ParentNeur.Delta - (int)IncRowsL);

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // Increasing X value by the amount of Delta value
                    ParentNeur.Neur_X += ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
            else // If neuron's position is to the right of the input array decrease coordinates' values by the amount of Delta value
            {
                // Calculate the increase value
                float IncRowsR = (ParentNeur.Neur_X - ParentNeur.Delta) / (MainWindow.Vpw - 2 * MainWindow._frame); //should be vpw - borders!

                if (IncRowsR < 0)
                {
                    // Decreasing Y by removing the integer part of Delta/X division (the amount of rows with width==X each)
                    ParentNeur.Neur_Y -= (int)(ParentNeur.Delta / ParentNeur.Neur_X) / MainWindow._pixelSize;

                    // Reducing Delta value by the amount of rows for further decrease of the X value
                    ParentNeur.Neur_X -= (ParentNeur.Delta - (ParentNeur.Neur_Y * ParentNeur.Neur_X));

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
                else
                {
                    // Decreasing X value by the amount of Delta value
                    ParentNeur.Neur_X -= ParentNeur.Delta;

                    // Assigning new Index value due to the change in coordinates
                    ParentNeur.NeurIndInput = MainWindow.CalculateIndex(ParentNeur.Neur_X, ParentNeur.Neur_Y);

                    // Changing coordinates for visualisation
                    ParentNeur.ChangePosition();
                }
            }
        }

        public void MoveNeighbours(long parent_id)   
        {
            Neuron ParentNeur;

            // reference to the parent neuron in the connection
            if (FindNeuron(parent_id) != null)
            {
                ParentNeur = FindNeuron(parent_id);
            }
            else
            {
                return;
            }

            // Find neighbours' ids for further search of any connections related to the parent neuron
            for (int i = 0; i < ParentNeur.AxonsWeights.Count; i++)
            {
                // Find neighbours' axons values related to the parent neuron for neighbours' position change towards MainInput
                Adapt_weights(ParentNeur.AxonsWeights[i].NeighId, false);
            }

            for (int i = 0; i < ParentNeur.SynapsesWeights.Count; i++)
            {
                Adapt_weights(ParentNeur.SynapsesWeights[i].NeighId, false);
            }
        }

    } //-->NeuralNetwork
}
