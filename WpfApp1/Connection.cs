using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Connection
    {
        // Connection id
        public int ConnId { get; set; }
        // First & second neurons in the connection
        public Neuron FirstNeurInConn;
        public Neuron SecondNeurInConn;
        // Ids of nodes that should be connected 
        public long Node_id1 { get; set; }
        public long Node_id2 { get; set; }
        // Connection weight & age
        public float ConnWeight { get; set; }
        public int ConnAge { get; set; }
        // Random value for new node's location
        private static Random rnd = new Random();

        // Connection between the first two neurons when we don't have the id of the parent neuron
        public Connection(ref long num)
        {
            ConnId = 0;
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            FirstNeurInConn = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Node_id1 = FirstNeurInConn.NeurId;

            var TempX_2 = MainWindow.Resln * (rnd.Next(0, 9));
            var TempY_2 = MainWindow.Resln * (rnd.Next(0, 9));
            SecondNeurInConn = new Neuron(++num, TempX_2, TempY_2);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            Node_id2 = SecondNeurInConn.NeurId;

            //adding output info to neur1
            FirstNeurInConn.AxonsWeights.Add(new ConnWeights(ConnId, 1, SecondNeurInConn.NeurId, TempX_2, TempY_2));

            //adding input info to neur2
            SecondNeurInConn.SynapsesWeights.Add(new ConnWeights(ConnId, 1, FirstNeurInConn.NeurId, TempX_1, TempY_1));

            MainWindow.GlRender();
        }

        // Creates connection when parent neuron already exists
        public Connection(ref Neuron parent_node, int previous_conn_id, ref long neur_num)
        {
            FirstNeurInConn = parent_node;
            long parent_node_id = parent_node.NeurId;
            float X_parent = parent_node.Neur_X;
            float Y_parent = parent_node.Neur_Y;
            
            ConnId = ++previous_conn_id;
            ConnAge = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            // adding new neuron with higher number value
            SecondNeurInConn = new Neuron(++neur_num, TempX_1, TempY_1);

            //send info about which neuron should be connected to the newly created neuron to the list of connections in the opengl mode 
            //(replace previous connection)
            MainWindow._ConnEdges.Add((ushort)(parent_node_id + (2 * parent_node_id)));
            Node_id1 = parent_node_id;

            MainWindow._ConnEdges.Add((ushort)(neur_num + (2 * neur_num)));
            Node_id2 = SecondNeurInConn.NeurId;

            //adding output info to the parent node, AxonsWeights.Count being the new id for the new weight that's being added
            FirstNeurInConn.AxonsWeights.Add(new ConnWeights(ConnId, FirstNeurInConn.AxonsWeights.Count + 1, SecondNeurInConn.NeurId,
                SecondNeurInConn.Neur_X, SecondNeurInConn.Neur_Y));

            // adding input info to the child node in the first connection 
            // "1" is the first weight_id for this neuron (SecondNeurInConn)
            SecondNeurInConn.SynapsesWeights.Add(new ConnWeights(ConnId, 1, FirstNeurInConn.NeurId, 
                FirstNeurInConn.Neur_X, FirstNeurInConn.Neur_Y));

            MainWindow.GlRender();
        }

        // Creates connection between two existing neurons
        public Connection (ref Neuron parent_node, ref Neuron child_node, int previous_conn_id)
        {
            long parent_node_id = parent_node.NeurId;
            long child_node_id = child_node.NeurId;
            
            ConnId = ++previous_conn_id;
            ConnAge = 0;

            FirstNeurInConn = parent_node;
            SecondNeurInConn = child_node;

            Node_id1 = parent_node_id;
            Node_id2 = child_node_id;

            //adding output info to axons array for the parent neuron, Axons.Count being new id for the new weight that's being added
            FirstNeurInConn.AxonsWeights.Add(new ConnWeights(ConnId, FirstNeurInConn.AxonsWeights.Count + 1, child_node_id, FirstNeurInConn.Neur_X, 
                FirstNeurInConn.Neur_Y));

            // adding input info to child node in the first connection 
            SecondNeurInConn.SynapsesWeights.Add(new ConnWeights(ConnId, SecondNeurInConn.SynapsesWeights.Count + 1, parent_node_id, SecondNeurInConn.Neur_X,
                SecondNeurInConn.Neur_Y));

            MainWindow._ConnEdges.Add((ushort)(parent_node_id + (2 * parent_node_id)));
            MainWindow._ConnEdges.Add((ushort)(child_node_id + (2 * child_node_id)));

            MainWindow.GlRender();
        }
                               
    } //-->Connection
}
