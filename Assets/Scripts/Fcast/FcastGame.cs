using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Osnowa.Osnowa.Unity;
using System.Linq;
using Zenject;

namespace Fcast {
public class FcastGame : MonoBehaviour
{
    public GameObject PlayerPrefab;

    private GameObject _player;
    private float _time = 0f;
    private float _interval = 2f;
    private DiContainer _container;
    private EntityViewBehaviour.Factory _factory;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("test" + FindObjectsOfType<EntityViewBehaviour>()
        //                .FirstOrDefault(e => e.Entity.isPlayerControlled));

        Debug.Log("test: " + (FindObjectsOfType<EntityViewBehaviour>()
            .FirstOrDefault(e => e.Entity.isPlayerControlled) == null ? "null!" : "found"));
        /*_player = FindObjectsOfType<EntityViewBehaviour>()
            .FirstOrDefault(e => e.Entity.isPlayerControlled)
            ?.gameObject;*/
        Debug.Log("test: " + (_player == null ? "null!" : "found"));
        var sceneContext = FindObjectOfType<Zenject.SceneContext>();
        _container = sceneContext.Container;
        if (_factory == null)
        {
            _factory = _container.Resolve<EntityViewBehaviour.Factory>();
/*
            _factory.OnCreated += (view) =>
            {
                _player = view.gameObject;
            };
*/
        }
        /*
        FcastGameLoop.It(new Game { Over = true, Player = _player, Tick = false });
        */
    }

    // Update is called once per frame
    void Update()
    {
/*
        Debug.Log("found count: " + _container.ResolveAll<EntityViewBehaviour>().Count);
        foreach (var obj in )
        {
            Debug.Log("found: " + obj.name);
        }
        _player = FindObjectsOfType<EntityViewBehaviour>()
                        .FirstOrDefault(v => v.name.Contains("EntityView"))?.gameObject;//Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
*/

        bool tick = false;
        _time += Time.deltaTime;
        if (_time >= _interval)
        {
            //Debug.Log("test: " + (_player == null ? "null!" : "found"));
            _time = 0f;
            tick = true;
        }
        // FcastGameLoop.It(new Game { Over = false, Player = _player, Tick = tick });
    }
}}
