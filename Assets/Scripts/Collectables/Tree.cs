using Unity.Netcode;
using UnityEngine;

namespace Collectables
{
    public class Tree : CollectableBase
    {
        public override CollectableType Type { get; set; } = CollectableType.Tree;
    }
}
