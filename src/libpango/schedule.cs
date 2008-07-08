using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule
    {
        // * maybe make it a singleton
        int time;
        // priority queue
        SortedList<int, Queue<EventHandler>> pqueue;

        public Schedule() {
            time = 0;
            pqueue = new SortedList<int, Queue<EventHandler>>();
        }
        public void add(EventHandler eh, int timeoffset) {
            int priority = time + timeoffset;
            if (!pqueue.ContainsKey(priority)) {
                pqueue.Add(priority, new Queue<EventHandler>());
                pqueue[priority].Enqueue(eh);
            }
        }
        public void callCurrentEvents() {
            if (pqueue.ContainsKey(time)) {
                EventHandler eh;
                Queue<EventHandler> queue = pqueue[time];
                while (queue.Count > 0) {
                    eh = queue.Dequeue();
                    //eh.Invoke(...)
                }
                pqueue.Remove(time);
            }
        }
    }
}
