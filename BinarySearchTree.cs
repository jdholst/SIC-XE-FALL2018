using System;
using System.Collections;
using System.Collections.Generic;

namespace SICXE
{
    /// <summary>
    /// Single node in a binary search tree.
    /// </summary>
    public class BSTNode<T>
    {
        public BSTNode<T> Left;
        public BSTNode<T> Right;
        public T Element;
    }

    /// <summary>
    /// Generic BST class. Uses a comparer to insert and search for nodes.
    /// </summary>
    public class BinarySearchTree<T> : IEnumerable<T>
    {
        private BSTNode<T> tree;
        private IComparer comparer;

        /// <summary>
        /// Total number of nodes in the tree.
        /// </summary>
        public int Count{ get; private set; }

        /// <summary>
        /// BST Constructor. Initializes tree to null. Sets the comparer to be used.
        /// </summary>
        public BinarySearchTree(IComparer comparer)
        {
            tree = null;
            Count = 0;
            this.comparer = comparer;
        }

        /// <summary>
        /// Displays the tree using LVR traversal
        /// </summary>
        public void InView()
        {
            InView(tree);
        }

        /// <summary>
        /// Displays the tree using VLR traversal
        /// </summary>
        public void PreView()
        {
            PreView(tree);
        }

        /// <summary>
        /// Displays the tree using VRL traversal
        /// </summary>
        public void PostView()
        {
            PostView(tree);
        }

        /// <summary>
        /// Inserts an element.
        /// </summary>
        /// <param name="element"></param>
        public void Insert(T element)
        {
            Insert(element, ref tree);
            Count++;
        }

        /// <summary>
        /// Removes an element.
        /// </summary>
        /// <param name="element"></param>
        public void Remove(T element)
        {
            Remove(element, ref tree);
        }

        /// <summary>
        /// Searches for an element
        /// </summary>
        /// <param name="element"> Search element</param>
        /// <param name="foundElement"> Element found in the search. 
        /// An empty element is set if failed </param>
        /// <returns> Returns true for a successful search </returns>
        public bool Search(T element, out T foundElement)
        {
            var foundNode = Search(element, tree);

            if (foundNode != null)
            {
                foundElement = foundNode.Element;
                return true;
            }
            else
            {
                // returns empty item
                foundElement = default(T);
                return false;
            }
        }

        /// <summary>
        /// Replaces an element.
        /// </summary>
        /// <param name="oldElement"> Search Element</param>
        /// <param name="newElement"> New Element </param>
        public void Replace(T oldElement, T newElement)
        {
            Replace(oldElement, newElement, ref tree);
        }

        /// <summary>
        /// Node is added by traversing it down the list, performing checks, 
        /// until it is on the bottom.
        /// </summary>
        /// <param name="element"> IN </param>
        /// <param name="root"> IN/OUT </param>
        private void Insert(T element, ref BSTNode<T> root)
        {
            var newNode = new BSTNode<T>
            {
                Left = null,
                Right = null,
                Element = element
            };

            if (root == null)
            {
                root = newNode;
            }
            else
            {
                if (comparer.Compare(element, root.Element) < 0)
                {
                    Insert(element, ref root.Left);
                }
                else if (comparer.Compare(element, root.Element) > 0)
                {
                    Insert(element, ref root.Right);
                }
            }
        }

        /// <summary>
        /// Searches the binary tree for the value, then removes the node.
        /// </summary>
        /// <param name="element"> IN </param>
        /// <param name="node"> IN/OUT </param>
        private void Remove(T element, ref BSTNode<T> node)
        {
            if (tree == null)
                throw new NullReferenceException("Cannot remove from an empty tree.");
            else
            {
                if (comparer.Compare(element, tree.Element) == 0)
                    RemoveNode(ref node);
                else if (comparer.Compare(element, tree.Element) < 0)
                    Remove(element, ref node.Left);
                else if (comparer.Compare(element, tree.Element) > 0)
                    Remove(element, ref node.Right);
            }
        }

        /// <summary>
        /// Searches the binary tree for the value, then changes the node with newElement.
        /// </summary>
        /// <param name="searchElement"> IN </param>
        /// <param name="newElement"> IN </param>
        /// <param name="node"></param>
        private void Replace(T searchElement, T newElement, ref BSTNode<T> tree)
        {
            if (tree != null)
            {

                if (comparer.Compare(searchElement, tree.Element) == 0)
                {
                    tree.Element = newElement;
                }
                else if (comparer.Compare(searchElement, tree.Element) < 0)
                    Replace(searchElement, newElement, ref tree.Left);
                else if (comparer.Compare(searchElement, tree.Element) > 0)
                    Replace(searchElement, newElement, ref tree.Right);
            }
        }

        /// <summary>
        /// Traverses down the tree until the value of the current node is
        /// equal to the value being passed in.
        /// </summary>
        /// <param name="element"> IN </param>
        /// <param name="tree"> IN </param>
        /// <returns> BSTNode </returns>
        private BSTNode<T> Search(T element, BSTNode<T> tree)
        {
            if (tree == null)
                return tree; //will return null
            else
            {
                BSTNode<T> searchValue = null;

                if (comparer.Compare(element, tree.Element) == 0)
                    return tree; //will return the node
                else if (comparer.Compare(element, tree.Element) < 0)
                    searchValue = Search(element, tree.Left);
                else if (comparer.Compare(element, tree.Element) > 0)
                    searchValue = Search(element, tree.Right);

                return searchValue;
            }
        }

        /// <summary>
        /// Displays the list using VLR traversal (Visit, go Left, go Right)
        /// Calls System.Object.ToString() to display.
        /// </summary>
        /// <param name="tree"> IN </param>
        private void PreView(BSTNode<T> tree)
        {
            if (tree != null)
            {
                Console.WriteLine(tree.Element.ToString());
                if (tree.Left != null)
                    InView(tree.Left);
                if (tree.Right != null)
                    InView(tree.Right);
            }
        }

        /// <summary>
        /// Displays the list using LVR traversal (go Left, Visit, go Right)
        /// Calls System.Object.ToString() to display.
        /// </summary>
        /// <param name="tree"> IN </param>
        private void InView(BSTNode<T> tree)
        {
            if (tree != null)
            {
                if (tree.Left != null)
                    InView(tree.Left);
                Console.WriteLine(tree.Element.ToString());
                if (tree.Right != null)
                    InView(tree.Right);
            }
        }

        /// <summary>
        /// Displays the list using LRV traversal (go Left, go Right, Visit)
        /// Calls System.Object.ToString() to display.
        /// </summary>
        /// <param name="tree"> IN </param>
        private void PostView(BSTNode<T> tree)
        {
            if (tree != null)
            {
                if (tree.Left != null)
                    InView(tree.Left);
                if (tree.Right != null)
                    InView(tree.Right);
                Console.WriteLine(tree.Element.ToString());
            }
        }


        /// <summary>
        /// Deletes the node passed in. Performs different operations depending
        /// on how many children the node has.
        /// </summary>
        /// <param name="node"> IN/OUT </param>
        private void RemoveNode(ref BSTNode<T> node)
        {
            BSTNode<T> tmp = null;

            if (node.Left == null && node.Right == null)
            {
                node = null;
            }
            else if (node.Left != null && node.Right == null)
            {
                tmp = node;
                node = node.Left;
                tmp.Left = null;
                tmp = null;
            }
            else if (node.Left == null && node.Right != null)
            {
                tmp = node;
                node = node.Right;
                tmp.Right = null;
                tmp = null;
            }
            else if (node.Left != null && node.Right != null)
            {
                FindMaxNode(ref node.Left, ref tmp);
                node.Element = tmp.Element;
                tmp = null;
            }
        }

        /// <summary>
        /// Finds the max node by traversing down the right pointers of the
        /// the tree until it is null.
        /// </summary>
        /// <param name="node"> IN/OUT </param>
        /// <param name="tmpNode"> IN/OUT </param>
        private void FindMaxNode(ref BSTNode<T> node, ref BSTNode<T> tmpNode)
        {
            if (node.Right == null)
            {
                tmpNode = node;
                node = node.Left;
                tmpNode.Right = null;
            }
            else
                FindMaxNode(ref node.Right, ref tmpNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var nodes = new List<T>();

            GetAllNodes(ref nodes, tree);

            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void GetAllNodes(ref List<T> nodes, BSTNode<T> tree)
        {
            if (tree != null)
            {
                nodes.Add(tree.Element);
                if (tree.Left != null)
                    GetAllNodes(ref nodes, tree.Left);
                if (tree.Right != null)
                    GetAllNodes(ref nodes, tree.Right);
            }
        }
    }
}

