using UnityEngine;

sealed class Switcher : MonoBehaviour
{
    [SerializeField] GameObject [] _targets = null;

    public void OnDropdownChanged(int index)
    {
        for (var i = 0; i < _targets.Length; i++)
            _targets[i].SetActive(i == index);
    }
}
