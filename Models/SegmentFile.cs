using System;
using System.IO;
using System.Runtime.InteropServices;
using SramCommons.Extensions;
using SramCommons.Helpers;
// ReSharper disable VirtualMemberCallInConstructor

namespace SramCommons.Models
{
	/// <summary>Provides load and save functionality for a generic <see c="StructFile{TStruct,TSegment}"/></summary>
	/// <typeparam name="TStruct">The file's structure type</typeparam>
	/// <typeparam name="TSegment">The file's segment structure type</typeparam>
	public class SegmentFile<TStruct, TSegment> : StructFile<TStruct>, ISegmentFile<TSegment>
		where TStruct : struct
		where TSegment : struct
	{
		/// <summary>The typed segment</summary>
		public virtual TSegment Segment { get; protected set; }

		/// <inheritdoc cref="SegmentFile{TSegment}.SegmentOffset"/>
		public virtual int SegmentOffset { get; }

		/// <inheritdoc cref="IMultiSegmentFile.SegmentSize"/>
		public int SegmentSize { get; }

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TStruct,TSegment}"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="stream">The stream the buffers will be loaded from</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		public SegmentFile(Stream stream, int segmentOffset) : this(Marshal.SizeOf<TStruct>(), segmentOffset) => Load(stream);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TStruct,TSegment}"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="buffer">The buffer which will be copied</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		public SegmentFile(byte[] buffer, int segmentOffset) : this(Marshal.SizeOf<TStruct>(), segmentOffset) => Load(buffer);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TStruct,TSegment}"/> and creates a buffer of the specified size
		/// </summary>
		/// <param name="size">The size of the buffer to create</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		public SegmentFile(int size, int segmentOffset) : base(size) => (SegmentOffset, SegmentSize) = (segmentOffset, Marshal.SizeOf<TSegment>());

		/// <inheritdoc cref="IBlobFile.Load"/>
		public override void Load(byte[] buffer)
		{
			base.Load(buffer);
			GetStructFromBlob();
		}

		/// <inheritdoc cref="IBlobFile.Load"/>
		public override void Load(Stream stream)
		{
			base.Load(stream);
			GetStructFromBlob();
		}

		private void GetStructFromBlob() => Segment = StructSerializer.Deserialize<TSegment>(Buffer);

		/// <inheritdoc cref="ISegmentFile{TSegment}.GetSegment"/>
		public virtual TSegment GetSegment() => Buffer[SegmentOffset..(SegmentOffset + Size)].ToStruct<TSegment>();

		/// <inheritdoc cref="ISegmentFile{TSegment}.SetSegment"/>
		public virtual void SetSegment(TSegment segment) => SetSegmentBytes(segment.ToBytes());

		/// <inheritdoc cref="ISegmentFile.GetSegmentBytes"/>
		public virtual byte[] GetSegmentBytes() => Buffer[SegmentOffset..(SegmentOffset + SegmentSize)];

		/// <inheritdoc cref="ISegmentFile.SetSegmentBytes"/>
		public virtual void SetSegmentBytes(byte[] buffer) => Array.Copy(buffer, 0, Buffer, SegmentOffset, SegmentSize);
	}

	/// <summary>Provides load and save functionality for a partial generic <see c="SegmentFile{TSegment}"/></summary>
	/// <typeparam name="TSegment">The file's segment structure type</typeparam>
	public class SegmentFile<TSegment> : StructFile<TSegment>, ISegmentFile<TSegment>
		where TSegment : struct
	{
		/// <inheritdoc cref="ISegmentFile.SegmentOffset"/>
		public int SegmentOffset { get; }
		/// <inheritdoc cref="ISegmentFile.SegmentSize"/>
		public int SegmentSize { get; }

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TSegment}"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="stream">The stream the buffers will be loaded from</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// /// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(Stream stream, int segmentOffset, int segmentSize) : this((int) stream.Length, segmentOffset,
			segmentSize) => Load(stream);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TSegment}"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="buffer">The buffer which will be copied</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// /// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(byte[] buffer, int segmentOffset, int segmentSize) : this(buffer.Length, segmentOffset, segmentSize) => Load(buffer);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile{TSegment}"/> and creates a buffer of the specified size
		/// </summary>
		/// <param name="size">The size of the buffer to create</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(int size, int segmentOffset, int segmentSize) : base(size) => (SegmentOffset, SegmentSize) = (segmentOffset, segmentSize);

		/// <inheritdoc cref="ISegmentFile{TSegment}.GetSegment"/>
		public virtual TSegment GetSegment() => Buffer[SegmentOffset..(SegmentOffset + Size)].ToStruct<TSegment>();

		/// <inheritdoc cref="ISegmentFile{TSegment}.SetSegment"/>
		public virtual void SetSegment(TSegment segment) => SetSegmentBytes(segment.ToBytes());

		/// <inheritdoc cref="ISegmentFile.GetSegmentBytes"/>
		public virtual byte[] GetSegmentBytes() => Buffer[SegmentOffset..(SegmentOffset + SegmentSize)];

		/// <inheritdoc cref="ISegmentFile.SetSegmentBytes"/>
		public virtual void SetSegmentBytes(byte[] buffer) => Array.Copy(buffer, 0, Buffer, SegmentOffset, SegmentSize);
	}

	/// <summary>Provides load and save functionality for a non-generic <see c="SegmentFile"/></summary>
	public class SegmentFile : BlobFile, ISegmentFile
	{
		/// <inheritdoc cref="ISegmentFile.SegmentOffset"/>
		public int SegmentOffset { get; }
		/// <inheritdoc cref="ISegmentFile.SegmentSize"/>
		public int SegmentSize { get; }

		/// <summary>
		/// Creates an instance of <see c="SegmentFile"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="stream">The stream the buffers will be loaded from</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// /// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(Stream stream, int segmentOffset, int segmentSize) : this((int)stream.Length, segmentOffset,
			segmentSize) => Load(stream);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile"/> and loads content from stream into buffer and struct
		/// </summary>
		/// <param name="buffer">The buffer which will be copied</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// /// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(byte[] buffer, int segmentOffset, int segmentSize) : this(buffer.Length, segmentOffset, segmentSize) => Load(buffer);

		/// <summary>
		/// Creates an instance of <see c="SegmentFile"/> and creates a buffer of the specified size
		/// </summary>
		/// <param name="size">The size of the buffer to create</param>
		/// <param name="segmentOffset">The offset of first segment in sram buffer</param>
		/// <param name="segmentSize">The size of the segment</param>
		public SegmentFile(int size, int segmentOffset, int segmentSize) : base(size) => (SegmentOffset, SegmentSize) = (segmentOffset, segmentSize);

		/// <inheritdoc cref="ISegmentFile.GetSegmentBytes"/>
		public virtual byte[] GetSegmentBytes() => Buffer[SegmentOffset..(SegmentOffset + SegmentSize)];

		/// <inheritdoc cref="ISegmentFile.SetSegmentBytes"/>
		public virtual void SetSegmentBytes(byte[] buffer) => Array.Copy(buffer, 0, Buffer, SegmentOffset, SegmentSize);
	}
}