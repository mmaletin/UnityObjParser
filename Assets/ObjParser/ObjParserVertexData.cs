
namespace Obj
{
    using System;

    public struct ObjParserVertexData : IEquatable<ObjParserVertexData> {

        public int vertex;
        public int uv;
        public int normal;

        public ObjParserVertexData(int vertex, int uv, int normal)
        {
            this.vertex = vertex;
            this.uv = uv;
            this.normal = normal;
        }

        public bool Equals(ObjParserVertexData other)
        {
            if (vertex != other.vertex) return false;
            if (uv != other.uv) return false;
            if (normal != other.normal) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjParserVertexData)) return false;

            return Equals((ObjParserVertexData)obj);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash += vertex;
                hash = hash * 23 + uv.GetHashCode();
                hash = hash * 23 + normal.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ObjParserVertexData a, ObjParserVertexData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObjParserVertexData a, ObjParserVertexData b)
        {
            return !(a == b);
        }
    }
}