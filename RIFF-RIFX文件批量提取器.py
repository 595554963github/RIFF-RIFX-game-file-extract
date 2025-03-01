import os
import struct

# 用于记录用户输入的音频格式，初始值为None
user_audio_format = None
# 用于标记是否已经输入过音频格式，初始值为False
audio_format_entered = False

def extract_data_blocks(file_content):
    riff_header = b'\x52\x49\x46\x46'
    rifx_header = b'\x52\x49\x46\x58'
    start_index = 0
    while True:
        riff_index = file_content.find(riff_header, start_index)
        rifx_index = file_content.find(rifx_header, start_index)
        if riff_index == -1 and rifx_index == -1:
            break
        current_index = riff_index if riff_index!= -1 and (rifx_index == -1 or riff_index < rifx_index) else rifx_index
        header = riff_header if current_index == riff_index else rifx_header

        file_size = struct.unpack('<I', file_content[current_index + 4:current_index + 8])[0]
        file_size = (file_size + 1) & ~1

        block_start = current_index + 8
        wave_format = b'\x57\x41\x56\x45\x66\x6D\x74'
        block_end_index = file_content.find(rifx_header if header == rifx_header else riff_header, current_index + file_size + 8)
        if block_end_index == -1:
            block_end_index = len(file_content)
        while block_start < block_end_index:
            current_block = file_content[block_start:block_start + 7]
            if current_block == b'\x46\x45\x56\x20\x46\x4D\x54':
                yield ('bank', file_content[current_index:current_index + file_size + 8])
            elif current_block == b'\x57\x45\x42\x50\x56\x50\x38':
                yield ('webp', file_content[current_index:current_index + file_size + 8])
            elif current_block == b'\x58\x57\x4D\x41\x66\x6D\x74':
                yield ('xwma', file_content[current_index:current_index + file_size + 8])
            elif current_block == wave_format:
                if header == riff_header:
                    global audio_format_entered
                    global user_audio_format
                    if not audio_format_entered:
                        valid_formats = ['at3', 'at9', 'wav', 'wem', 'xma']
                        user_audio_format = input(f"请输入要保存的格式({', '.join(valid_formats)}): ")
                        while user_audio_format not in valid_formats:
                            user_audio_format = input(f"无效格式，请重新输入要保存的格式({', '.join(valid_formats)}): ")
                        audio_format_entered = True
                    yield (user_audio_format, file_content[current_index:current_index + file_size + 8])
                else:
                    yield ('wem', file_content[current_index:current_index + file_size + 8])
            block_start += 7
        start_index = current_index + file_size + 8

def extract_from_file(file_path):
    with open(file_path, 'rb') as file:
        file_content = file.read()

    data_generator = extract_data_blocks(file_content)
    count = 0
    base_filename = os.path.splitext(os.path.basename(file_path))[0]
    for target_extension, data in data_generator:
        extracted_filename = f"{base_filename}_{count}.{target_extension}"
        extracted_path = os.path.join(os.path.dirname(file_path), extracted_filename)
        os.makedirs(os.path.dirname(extracted_path), exist_ok=True)
        with open(extracted_path, 'wb') as output_file:
            output_file.write(data)
        print(f"Extracted content saved as: {extracted_path}")
        count += 1

def extract(directory_path):
    for root, dirs, files in os.walk(directory_path):
        for file in files:
            if file.endswith('.py'):
                continue
            file_path = os.path.join(root, file)
            extract_from_file(file_path)

def main():
    directory_path = input("请输入要处理的文件夹路径: ")
    if not os.path.isdir(directory_path):
        print(f"错误: {directory_path} 不是一个有效的目录。")
        return

    extract(directory_path)
    print("文件提取完成。")

if __name__ == "__main__":
    main()