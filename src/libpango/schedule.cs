using System;
using System.Collections.Generic;
using System.Text;

namespace libpango
{
    public class Schedule
    {
        public delegate void EventDelegate();

        private static Schedule instance; // a singleton
        private int time;
        // priority queue
        SortedList<int, Queue<EventDelegate>> pqueue;

        private Schedule() {
            time = 0;
            pqueue = new SortedList<int, Queue<EventDelegate>>();
        }
        public static Schedule Instance {
            get {
                if(instance == null) {
                    instance = new Schedule();
                }
                return instance;
            }
        }
        public void add(EventDelegate eh, int timeoffset) {
            int priority = time + timeoffset;
            if (!pqueue.ContainsKey(priority)) {
                pqueue.Add(priority, new Queue<EventDelegate>());
                pqueue[priority].Enqueue(eh);
            }
        }
        public void callCurrentEvents() {
            if (pqueue.ContainsKey(time)) {
                EventDelegate ed;
                Queue<EventDelegate> queue = pqueue[time];
                while (queue.Count > 0) {
                    ed = queue.Dequeue();
                    ed(); // perform the scheduled action
                }
                pqueue.Remove(time);
            }
        }
        public int Time {
            get { return time; }
        }
        public void increaseTime() {
            time++;
        }
    }
}
