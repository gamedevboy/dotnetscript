namespace DotNetScript.Types.Reference
{
    internal struct ScriptArrayReference : IScriptReference
    {
        private readonly int _index;
        private readonly object[] _array;

        public ScriptArrayReference(object[] array, int index)
        {
            _array = array;
            _index = index;
        }

        public object Value {
            get { return _array[_index]; }
            set { _array[_index] = value; }
        }
    }
}