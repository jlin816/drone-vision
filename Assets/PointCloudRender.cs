using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System;

namespace AssemblyCSharp {
	
	public class PointCloudRender : MonoBehaviour {
		//public GameObject gameObj;
		private bool streaming;
		private const string URL = "http://mhacksv.str.at/query.php";
		
		public int GetNthIndex(string s, char t, int n)
		{
			int count = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == t)
				{
					count++;
					if (count == n)
					{
						return i;
					}
				}
			}
			return -1;
		}
		// Use this for initialization
		void Start () {
			//streaming = true;
			Debug.Log ("Starting...");
		}
		
		// Update is called once per frame
		void Update () {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (URL);
			request.Method = WebRequestMethods.Http.Get;
			request.ContentType = "application/json";
			HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			StreamReader reader = new StreamReader(response.GetResponseStream ());
			string json = reader.ReadToEnd();
			string xyz_count_s = json.Substring(GetNthIndex(json,'"',3)+1,GetNthIndex(json,'"',4)-GetNthIndex(json,'"',3)-1);
			string xyz_parcel_s = json.Substring(GetNthIndex(json,'"',7)+1,GetNthIndex(json,'"',8)-GetNthIndex(json,'"',7)-1).Replace("\\","");
			if (xyz_parcel_s.Length % 4 == 2)
								xyz_parcel_s += "==";
						else if (xyz_parcel_s.Length % 4 == 3)
								xyz_parcel_s += "=";
			string timestamp_s = json.Substring(GetNthIndex(json,'"',11)+1,GetNthIndex(json,'"',12)-GetNthIndex(json,'"',11)-1);

			//Debug.Log (xyz_count_s);
			Debug.Log (xyz_parcel_s.Length);
			Debug.Log (xyz_parcel_s.Substring(xyz_parcel_s.Length-5,5));
			//Debug.Log (timestamp_s);
			TangoStruct tangoStruct = new TangoStruct(float.Parse(timestamp_s),Int32.Parse(xyz_count_s),xyz_parcel_s.ToCharArray(0,xyz_parcel_s.Length));
			
			//EDIT
			//int dimx = 0; //ij_cols -- NUM POINTS not side length
			//int dimy = 0; //ij_rows
			//int[][] ij_parcel = new int[dimx][dimy];
			int xyz_count = tangoStruct.xyz_count;
			byte[] xyz_parcel_bin = System.Convert.FromBase64String(new string(tangoStruct.xyz_parcel));

			
			MemoryStream mstream = new MemoryStream(xyz_parcel_bin);
			
			Mesh mesh = new Mesh();
			GetComponent<MeshFilter>().mesh = mesh;// sets MeshFilter's mesh, passes to MeshRenderer
			int[] idxs = new int[xyz_count];
			Vector3[] xyz_parcel = new Vector3[xyz_count];
			for(int i=0;i<xyz_count;i++)
			{
				idxs[i] = i;
				byte[] cFloat = new byte[12];
				mstream.Read(cFloat,0,12);
				xyz_parcel[i] = new Vector3(BitConverter.ToSingle(cFloat,0),BitConverter.ToSingle (cFloat,4),BitConverter.ToSingle(cFloat,8));
				Debug.Log(BitConverter.ToSingle(cFloat,0) + "," + BitConverter.ToSingle (cFloat,4) + "," + BitConverter.ToSingle(cFloat,8));
			}
			
			
			mesh.vertices = xyz_parcel;
			/*
		triangles = new Vector3[dimx * dimy-dimx-dimy+1];
		int ntri = 0;

		Vector3[][] xyz_ordered= new Vector3[dimx][dimy];
		for(int i = 0; i < dimx; i++)
		{
			for(int j = 0; j < dimy; j++)
			{
				if(ij_parcel[i][j]!=-1)
					xyz_ordered[i][j] = xyz_parcel[ij_parcel[i][j]];
				else
				{
					if(j>1)
						xyz_ordered[i][j] = xyz_ordered[i][j-1];
					else if (i>1) 
						xyz_ordered[i][j] = xyz_ordered[i-1][j];
					else
						xyz_ordered[i][j] = Vector3(0,0,0);
				}
			}
		}

		//upper diagonal triangles
		for (int r = 0; r <= dimy-1; r++) 
		{
			for (int c = 0; c <= dimx-1; c++) {
				mesh.triangles [3 * ntri] = xyz_ordered[r][c];
				mesh.triangles [3 * ntri + 1] = xyz_ordered[r][c+1];
				mesh.triangles [3 * ntri + 2] = xyz_ordered[r+1][c];
		
				ntri++;
			}
		}

		//lower diagonal triangles
		for (int r = dimy; r>=1; r--)
		{
			for (int c = dimx; c>=1; c--)
			{
				mesh.triangles[3*ntri] = xyz_ordered[r][c];
				mesh.triangles[3*ntri+1] = xyz_ordered[r-1][c];
				mesh.triangles[3*ntri+2] = xyz_ordered[r][c-1];

				ntri++;
			}
		}
		*/
			
			mesh.SetIndices(idxs,MeshTopology.Points,0);
			GetComponent<MeshFilter> ().mesh = mesh;
			//mesh.triangles = triangles;
			//mesh.RecalculateNormals();
			//mesh.Optimize();
		}
	}
}