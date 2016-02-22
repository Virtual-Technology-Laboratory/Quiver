/*
 * Copyright (c) 2014, Roger Lew (rogerlew.gmail.com)
 * Date: 2/5/2015
 * License: BSD (3-clause license)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VectorFieldResourceLoader
{

    public static List<List<Vector2>> Load(string resourceLocation)
    {
        TextAsset asset = Resources.Load(resourceLocation) as TextAsset;
        var uv = new List<List<Vector2>>();

        int m = System.BitConverter.ToInt32(asset.bytes, 0);
        int n = System.BitConverter.ToInt32(asset.bytes, 4);

        int k = 8;
        for (int i = 0; i < m; i++)
        {
            uv.Add(new List<Vector2>());
            for (int j = 0; j < n; j++)
            {
                var u = System.BitConverter.ToSingle(asset.bytes, k);
                var v = System.BitConverter.ToSingle(asset.bytes, k + 4);
                k += 8;

                uv[i].Add(new Vector2(u, v));
            }
        }
        return uv;
    }

    static IEnumerable<byte[]> Split(byte splitByte, byte[] buffer)
    {
        List<byte> bytes = new List<byte>();
        foreach (byte b in buffer)
        {
            if (b != splitByte)
                bytes.Add(b);
            else
            {
                yield return bytes.ToArray();
                bytes.Clear();
            }
        }
        yield return bytes.ToArray();
    }
}
