using System.Collections.Generic;

using MapTileGridCreator.Core;
using MapTileGridCreator.Utilities;

using UnityEngine;

namespace MapTileGridCreator.TransformationsBank
{

	[CreateAssetMenu(fileName = "modif_FilterArityByLayer", menuName = "MapTileGridCreator/Modifiers/FilterArityByLayer")]
	public class ModifierFilterArityByLayer : Modifier
	{

		#region Inspector
#pragma warning disable 0649

		[Header("ModifierFilterArityByLayer")]

		[SerializeField]
		[Tooltip("The maximum arity of a given cell for filtering.")]
		private int _high_pass_arity = 3;

		[SerializeField]
		[Tooltip("The minimum arity of a given cell for filtering.")]
		private int _low_pass_arity = 0;

#pragma warning restore 0649
		#endregion

		private Queue<Core.Cell> _to_delete = new Queue<Core.Cell>();

		protected override void BeforeModify(Grid3D grid, Vector3Int index)
		{
			_to_delete.Clear();
		}

		protected override void AfterModify(Grid3D grid)
		{
			foreach (Core.Cell c in _to_delete)
			{
                FuncEditor.DestroyCell(c);
			}
			last_index = last_index + Vector3Int.up;
		}

		public override Queue<Vector3Int> Modify(Grid3D grid, Vector3Int index)
		{
			Queue<Vector3Int> newIndexes = new Queue<Vector3Int>();

            Core.Cell root = grid.TryGetCellByIndex(ref index);
			Vector3Int upIndex = index + Vector3Int.up;
            Core.Cell up = grid.TryGetCellByIndex(ref upIndex);

			if (root == null)
			{
				return null;
			}

            List<Core.Cell> neighb = grid.GetNeighboursCell(ref index);
			foreach (Core.Cell cell in neighb)
			{
				if (cell.GetIndex().y == index.y)
				{
					newIndexes.Enqueue(cell.GetIndex());
				}
			}

			Vector3Int downIndex = index + Vector3Int.down;
            Core.Cell down = grid.TryGetCellByIndex(ref downIndex);

			int otherLayerCount = up != null ? down != null ? 2 : 1 : 0;

			int arity = (neighb.Count - otherLayerCount);
			if (_low_pass_arity < arity && arity <= _high_pass_arity)
			{
				return newIndexes;
			}

			if (up != null)
			{
				_to_delete.Enqueue(root);
			}

			return newIndexes;
		}
	}
}
