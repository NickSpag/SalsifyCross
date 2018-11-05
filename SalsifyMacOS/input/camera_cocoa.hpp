//
//  camera_cocoa.hpp
//  SalsifyMacOS
//
//  Created by Nicholas Spagnola on 10/24/18.
//  Copyright © 2018 Nicholas Spagnola. All rights reserved.
//

#ifndef camera_cocoa_hpp
#define camera_cocoa_hpp

#include <stdio.h>

//NICK -linux's v4l2 library. replacing this with the cocoa camera apis
//#include <linux/videodev2.h>

//required from interface definitions
#include "frame_input.hh"
#include "raster_handle.hh"
#include "optional.hh"

#include "file_descriptor.hh"
#include "mmap_region.hh"

//static const std::unordered_map<std::string, uint32_t> PIXEL_FORMAT_STRS {
//    { "NV12", V4L2_PIX_FMT_NV12 },
//    { "YUYV", V4L2_PIX_FMT_YUYV },
//    { "YU12", V4L2_PIX_FMT_YUV420 }
//};

class CameraCocoa : public FrameInput
{
    
private:
    
    uint16_t width_;
    uint16_t height_;
    
    FileDescriptor camera_fd_;
    std::unique_ptr<MMap_Region> mmap_region_;
    
    uint32_t pixel_format_;
    //from linux v4l2 lib
    //v4l2_buffer buffer_info_;
    int type_;
    
public:
    //looks like the implementation's constructor with the v4l2 specific format enum or whatever and some device string
    CameraCocoa(const uint16_t width, const uint16_t height,
                const uint32_t pixel_format = V4L2_PIX_FMT_YUV420,
                const std::string device = "/dev/video0" );

    ~CameraCocoa();
    
//interface properties
    Optional<RasterHandle> get_next_frame() override;
    uint16_t display_width() { return width_; }
    uint16_t display_height() { return height_; }

//the linux one has the below- but its not part of the interface
    FileDescriptor & fd() { return camera_fd_; }
    
};

#endif /* camera_cocoa_hpp */
