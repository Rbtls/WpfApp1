using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Connection
    {
        public Neuron _neur1 { get; set; }
        public Neuron _neur2 { get; set; }
        public float _weight { get; set; }
        public int Age { get; set; }
        //ids of nodes that should be connected 
        public long _id1 { get; set; } 
        public long _id2 { get; set; }
        private static Random rnd = new Random();

        //connection between first two neurons when we don't have the id of the parent neuron
        public Connection(ref long num)
        {
            Age = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            _neur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id1 = _neur1.NeurNum;

            var TempX_2 = MainWindow.Resln * (rnd.Next(0, 9));
            var TempY_2 = MainWindow.Resln * (rnd.Next(0, 9));
            _neur2 = new Neuron(++num, TempX_2, TempY_2);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id2 = _neur2.NeurNum;

            //adding output info to neur1
            _neur1.Axons.Add(new NumWeights(1, _neur2.NeurNum, TempX_2, TempY_2));

            //adding input info to neur2
            _neur2.Synapses.Add(new NumWeights(1, _neur1.NeurNum, TempX_1, TempY_1));

            MainWindow.GlRender();
        }

        // Creating connection when parent neuron already exists
        public Connection(ref long num, long previous_id, float X_parent, float Y_parent)
        {
            Age = 0;
            var TempX_1 = MainWindow.Resln * (rnd.Next(1, 10));
            var TempY_1 = MainWindow.Resln * (rnd.Next(1, 10));
            _neur1 = new Neuron(++num, TempX_1, TempY_1);
            MainWindow._ConnEdges.Add((ushort)(num + (2 * num)));
            _id1 = _neur1.NeurNum;

            //adding output info to _neur1.Axons array, Axons.Count being new id for the new weight that's being added
            _neur1.Axons.Add(new NumWeights(_neur1.Axons.Count, previous_id, X_parent, Y_parent));

            //send info (about what neuron should be connected to _neur1) to the list of connections in the opengl mode
            MainWindow._ConnEdges.Add((ushort)(previous_id + (2 * previous_id)));
            _id2 = previous_id;

            MainWindow.GlRender();
        }
    } //-->Connection
}
