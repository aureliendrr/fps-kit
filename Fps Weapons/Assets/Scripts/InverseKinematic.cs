using UnityEditor;
using UnityEngine;

public class InverseKinematic : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _drawGizmos;
    [Range(0.01f, 1f)]
    [SerializeField] private float _targetSize;
    [Range(0.01f, 1f)]
    [SerializeField] private float _poleSize;

    [Header("Chain")]
    [Tooltip("Chain length of bones")]
    [SerializeField] private int _chainLength = 2;
    [Tooltip("Target the chain should bent to")]
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _pole;

    [Header("Solver")]
    [Tooltip("Iterations per update")]
    [SerializeField] private int _iterations = 10;
    [Tooltip("Distance when the solver stop")]
    [SerializeField] private float _delta = 0.001f;

    [Tooltip("Strength of going back to the start position")]
    [Range(0, 1)]
    [SerializeField] private float _snapBackStrength = 1f;

    private float[] _bonesLength;
    private float _completeLength;
    private Transform[] _bones;
    private Vector3[] _positions;
    private Vector3[] _startDirectionSucc;
    private Quaternion[] _startRotationBone;
    private Quaternion _startRotationTarget;
    private Quaternion _startRotationRoot;
    private Transform _root;

    private void Awake()
    {
        Init();   
    }

    private void Init()
    {
        //init vars
        _bones = new Transform[_chainLength + 1];
        _positions = new Vector3[_chainLength + 1];
        _bonesLength = new float[_chainLength];
        _startDirectionSucc = new Vector3[_chainLength + 1];
        _startRotationBone = new Quaternion[_chainLength + 1];

        //find root
        _root = transform;
        for (var i = 0; i <= _chainLength; i++)
        {
            if (_root == null)
                throw new UnityException("The chain value is longer than the ancestor chain!");
            _root = _root.parent;
        }

        if(_target == null)
        {
            _target = new GameObject(gameObject.name + " Target").transform;
            SetPositionRootSpace(_target, GetPositionRootSpace(transform));
        }

        _startRotationTarget = GetRotationRootSpace(_target);

        //init data
        var current = transform;
        _completeLength = 0;
        for (var i = _bones.Length - 1; i >= 0; i--)
        {
            _bones[i] = current;
            _startRotationBone[i] = GetRotationRootSpace(current);

            if(i == _bones.Length - 1)
            {
                _startDirectionSucc[i] = GetPositionRootSpace(_target) - GetPositionRootSpace(current);
            }
            else
            {
                _startDirectionSucc[i] = GetPositionRootSpace(_bones[i + 1]) - GetPositionRootSpace(current);
                _bonesLength[i] = _startDirectionSucc[i].magnitude;
                _completeLength += _bonesLength[i];
            }

            current = current.parent;
        }
    }

    public void UpdateTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void ResolveIK()
    {
        if (_target == null)
            return;

        if (_bonesLength.Length != _chainLength)
            Init();

        //Fabric

        //  root
        //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
        //   x--------------------x--------------------x---...

        //get position
        for (int i = 0; i < _bones.Length; i++)
            _positions[i] = GetPositionRootSpace(_bones[i]);

        var targetPosition = GetPositionRootSpace(_target);
        var targetRotation = GetRotationRootSpace(_target);

        //1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(_bones[0])).sqrMagnitude >= _completeLength * _completeLength)
        {
            //just strech it
            var direction = (targetPosition - _positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < _positions.Length; i++)
                _positions[i] = _positions[i - 1] + direction * _bonesLength[i - 1];
        }
        else
        {
            for (int i = 0; i < _positions.Length - 1; i++)
                _positions[i + 1] = Vector3.Lerp(_positions[i + 1], _positions[i] + _startDirectionSucc[i], _snapBackStrength);

            for (int iteration = 0; iteration < _iterations; iteration++)
            {
                //back
                for (int i = _positions.Length - 1; i > 0; i--)
                {
                    if (i == _positions.Length - 1)
                        _positions[i] = targetPosition; //set it to target
                    else
                        _positions[i] = _positions[i + 1] + (_positions[i] - _positions[i + 1]).normalized * _bonesLength[i]; //set in line on distance
                }

                //forward
                for (int i = 1; i < _positions.Length; i++)
                    _positions[i] = _positions[i - 1] + (_positions[i] - _positions[i - 1]).normalized * _bonesLength[i - 1];

                //close enough?
                if ((_positions[_positions.Length - 1] - targetPosition).sqrMagnitude < _delta * _delta)
                    break;
            }
        }

        //move towards pole
        if (_pole != null)
        {
            var polePosition = GetPositionRootSpace(_pole);
            for (int i = 1; i < _positions.Length - 1; i++)
            {
                var plane = new Plane(_positions[i + 1] - _positions[i - 1], _positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(polePosition);
                var projectedBone = plane.ClosestPointOnPlane(_positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - _positions[i - 1], projectedPole - _positions[i - 1], plane.normal);
                _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[i] - _positions[i - 1]) + _positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < _positions.Length; i++)
        {
            if (i == _positions.Length - 1)
                SetRotationRootSpace(_bones[i], Quaternion.Inverse(targetRotation) * _startRotationTarget * Quaternion.Inverse(_startRotationBone[i]));
            else
                SetRotationRootSpace(_bones[i], Quaternion.FromToRotation(_startDirectionSucc[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotationBone[i]));
            SetPositionRootSpace(_bones[i], _positions[i]);
        }
    }

    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (_root == null)
            return current.position;
        else
            return Quaternion.Inverse(_root.rotation) * (current.position - _root.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (_root == null)
            current.position = position;
        else
            current.position = _root.rotation * position + _root.position;
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before => rot: before -> after
        if (_root == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * _root.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (_root == null)
            current.rotation = rotation;
        else
            current.rotation = _root.rotation * rotation;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if(!_drawGizmos) return;

        var current = this.transform;
        for (int i = 0; i < _chainLength && current != null && current.parent != null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position),
                new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }

        if(_target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_target.position, new Vector3(.1f, .1f, .1f) * _targetSize);
        }

        if(_pole != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_pole.position, new Vector3(.1f, .1f, .1f) * _poleSize);
        }
        #endif
    }
}
