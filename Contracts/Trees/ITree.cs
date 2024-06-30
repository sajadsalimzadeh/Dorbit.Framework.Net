using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.Trees;

public interface ITree<T> where T : ITree<T>
{
    List<T> Children { get; set; }
}