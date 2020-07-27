using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Tree
{
    [Serializable]
	public class TreeElement : ScriptableObject
    {
		[SerializeField] int _ID;
		[SerializeField] string _Name;
        [SerializeField] int _Depth;
		[NonSerialized] TreeElement _Parent;
		[NonSerialized] List<TreeElement> _Children;

        [JsonIgnore]
		public TreeElement Parent
		{
			get { return _Parent; }
			set { _Parent = value; }
		}

        [JsonIgnore]
        public List<TreeElement> Children
		{
			get { return _Children; }
			set { _Children = value; }
		}

		public bool HasChildren
		{
			get { return Children != null && Children.Count > 0; }
		}

		public string Name
		{
			get { return _Name; } set { _Name = value; }
		}

		public int ID
		{
			get { return _ID; } set { _ID = value; }
		}

        public int Depth { get => _Depth; set => _Depth = value; }

        public TreeElement ()
		{}

		public TreeElement (string name, int depth, int id)
		{
			_Name = name;
			_ID = id;
            _Depth = depth;
		}
    }
}
