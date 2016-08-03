using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets.Game
{
    public class RoomGenerator : MonoBehaviour
    {
        public class QuadNode
        {
            public List<QuadNode> l = new List<QuadNode>();
            public List<QuadNode> r = new List<QuadNode>();
            public List<QuadNode> f = new List<QuadNode>();
            public List<QuadNode> b = new List<QuadNode>();
            public uint x, y;
            public uint w, h;
            public GameObject room;
            public override string ToString()
            {
                return string.Format("Node({0}, {1}, {2}, {3})", x, y, w, h);
            }
        }

        public uint width = 1000;
        public uint height = 1000;
        public uint minSize = 10;
        public uint wallWidth = 1;
        public GameObject roomProto = null;

        int random(int min, int max)
        {
            return min + (int)(Random.value * (max - min));
        }

        /// <summary>
        /// 방 쪼개기
        /// </summary>
        /// 가로 / 세로 / 분할 안함 중에서 하나를 수행
        /// <param name="node">분할하려는 방 노드</param>
        /// <returns>방이 분할된 경우 새로 만들어진 방의 노드를 리턴</returns>
        QuadNode SplitNode(QuadNode node)
        {
            // 0: l, r
            // 1: f, b
            // 2: no split
            uint size;
            QuadNode newNode = null;
            List<QuadNode> tempLsit = new List<QuadNode>();
            switch (random(0, 2))
            {
                case 0:
                    // 사이즈가 너무 작은 경우를 피하기 위해서 적당한 값으로 처리
                    size = (uint)random((int)node.w / 4, (int)(node.w - node.w / 4));
                    if (size < minSize || node.w - size < minSize)
                    {
                        return null;
                    }
                    newNode = new QuadNode
                    {
                        x = node.x + size + wallWidth,
                        y = node.y,
                        w = node.w - size - wallWidth,
                        h = node.h
                    };
                    node.w = size;

                    // 인접 방 간에 연결관계 재설정
                    newNode.l.Add(node);
                    newNode.r.AddRange(node.r);

                    foreach(var n in node.r)
                    {
                        n.l[n.l.IndexOf(node)] = newNode;
                    }
                    node.r.Clear();
                    node.r.Add(newNode);

                    foreach(var sn in node.f)
                    {
                        var r = sn.x + sn.w;
                        if (sn.x > newNode.x)
                        {
                            tempLsit.Add(sn);
                        }
                        if (r > newNode.x)
                        {
                            newNode.f.Add(sn);
                            sn.b.Insert(sn.b.IndexOf(node), newNode);
                        }
                    }
                    foreach(var n in tempLsit)
                    {
                        n.b.Remove(node);
                        node.f.Remove(n);
                    }
                    tempLsit.Clear();

                    foreach (var sn in node.b)
                    {
                        var r = sn.x + sn.w;
                        if (sn.x > newNode.x)
                        {
                            tempLsit.Add(sn);
                        }
                        if (r > newNode.x)
                        {
                            newNode.b.Add(sn);
                            sn.f.Insert(sn.f.IndexOf(node), newNode);
                        }
                    }
                    foreach (var n in tempLsit)
                    {
                        n.f.Remove(node);
                        node.b.Remove(n);
                    }
                    break;
                case 1:
                    // 사이즈가 너무 작은 경우를 피하기 위해서 적당한 값으로 처리
                    size = (uint)random((int)node.h / 4, (int)(node.h - node.h / 4));
                    if (size < minSize || node.w - size < minSize)
                    {
                        return null;
                    }
                    newNode = new QuadNode
                    {
                        x = node.x,
                        y = node.y + size + wallWidth,
                        w = node.w,
                        h = node.h - size - wallWidth
                    };
                    node.h = size;

                    // 인접 방 간에 연결관계 재설정
                    newNode.b.Add(node);
                    newNode.f.AddRange(node.f);

                    foreach (var n in node.f)
                    {
                        n.b[n.b.IndexOf(node)] = newNode;
                    }

                    node.f.Clear();
                    node.f.Add(newNode);

                    foreach (var sn in node.l)
                    {
                        var r = sn.x + sn.w;
                        if (sn.x > newNode.x)
                        {
                            tempLsit.Add(sn);
                        }
                        if (r > newNode.x)
                        {
                            newNode.l.Add(sn);
                            sn.r.Insert(sn.r.IndexOf(node), newNode);
                        }
                    }
                    foreach (var n in tempLsit)
                    {
                        n.r.Remove(node);
                        node.l.Remove(n);
                    }
                    tempLsit.Clear();

                    foreach (var sn in node.r)
                    {
                        var r = sn.x + sn.w;
                        if (sn.x > newNode.x)
                        {
                            tempLsit.Add(sn);
                        }
                        if (r > newNode.x)
                        {
                            newNode.r.Add(sn);
                            sn.l.Insert(sn.l.IndexOf(node), newNode);
                        }
                    }
                    foreach (var n in tempLsit)
                    {
                        n.l.Remove(node);
                        node.r.Remove(n);
                    }
                    break;
                case 2:
                    // 분할 안함
                    return null;
            }
            Debug.Log("-------");
            Debug.Log(node);
            Debug.Log(newNode);
            Debug.Log("-------");
            return newNode;
        }

        // Use this for initialization
        void Start()
        {
            // 일단 좌우상하 대칭을 생각하고 1/4 조각으로 쪼갬
            // 나중에 이걸 기준으로 대칭 생성하면 될거임
            QuadNode centerNode = new QuadNode
            {
                w = width / 2,
                h = height / 2,
                x = 0,
                y = 0
            };
            Queue<QuadNode> queue = new Queue<QuadNode>();
            queue.Enqueue(centerNode);
            while(queue.Count > 0)
            {
                // 쪼갠다!
                QuadNode node = queue.Dequeue();
                QuadNode newNode = SplitNode(node);

                // 쪼갰으면 또 쪼개본다!
                if (newNode != null)
                {
                    // 조금 더 적절히 하려면 여기서 크기가 큰 것을 우선 처리하도록 하면 될 듯
                    // 방 갯수 제한을 건다던가 했을 때를 고려
                    queue.Enqueue(node);
                    queue.Enqueue(newNode);
                }
            }

            // 미리보기 생성 - 적당히 패널로 만들어봄
            queue.Enqueue(centerNode);
            while (queue.Count > 0)
            {
                QuadNode node = queue.Dequeue();
                if (node.room != null)
                {
                    continue;
                }

                node.room = Instantiate(roomProto);
                node.room.transform.parent = transform;
                var rt = node.room.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(node.x, node.y);
                rt.sizeDelta = new Vector2(node.w, node.h);
                node.room.GetComponent<Image>().color = new Color(Random.value, Random.value, Random.value);
                node.room.name = string.Format(node.ToString());

                foreach (var n in node.l)
                {
                    queue.Enqueue(n);
                }
                foreach (var n in node.r)
                {
                    queue.Enqueue(n);
                }
                foreach (var n in node.f)
                {
                    queue.Enqueue(n);
                }
                foreach (var n in node.b)
                {
                    queue.Enqueue(n);
                }
            }
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}