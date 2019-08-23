using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommandProcessors
{
    public interface ICommand
    {
        void Perform();
        void Execute();
    }
    
    public class BaseCommand: ICommand
    {
        public virtual void Perform()
        {

        }

        public virtual void Execute()
        {

        }
    }

    public interface IProcessor
    {
        void AddCommand(ICommand command);
        void RemoveCommand(ICommand command);
        void Process();
    }

    public class Processor: IProcessor
    {
        protected LinkedList<ICommand> fCommands = new LinkedList<ICommand>();

        public void AddCommand(ICommand command)
        {
            fCommands.AddLast(command);
        }

        public void RemoveCommand(ICommand command)
        {
            fCommands.Remove(command);
        }

        public void Process()
        {
            LinkedListNode<ICommand> node = fCommands.First;

            if (node != null)
            {
                node.Value.Execute();
                fCommands.Remove(node);
            }
        }
    }

}