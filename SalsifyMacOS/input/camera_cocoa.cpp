//
//  camera_cocoa.cpp
//  SalsifyMacOS
//
//  Created by Nicholas Spagnola on 10/24/18.
//  Copyright © 2018 Nicholas Spagnola. All rights reserved.
//

#include <iostream>
#include <memory>
#include <unordered_set>
#include <cstdio>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <sys/mman.h>

#include "camera_cocoa.hpp"
#include "exception.hh"

using namespace std;

//https://afrantzis.com/pixel-format-guide/v4l2.html
//video for linux pixel formats ^
unordered_set<uint32_t> SUPPORTEDFORMATS{
    //list of strings or enums donating V4l2's pixel formats - uses YUV420 by default i think
  //{ V4L2_PIX_FMT_NV12, V4L2_PIX_FMT_YUYV, V4L2_PIX_FMT_YUV420 }
};

CameraCocoa::CameraCocoa( const uint16_t width, const uint16_t height,
                         const uint32_t pixel_format, const string device )
    : width_( width ), height_( height ),
    camera_fd_( SystemCall( "open camera", open( device.c_str(), O_RDWR ) ) ),
//buffer info in the header is vr12_buffer type from the linux library
    mmap_region_(), pixel_format_( pixel_format ), buffer_info_(), type_()
{
    
}

CameraCocoa::~CameraCocoa()
{
    //this throws an error with the string, assumign the second value is above 0. its an exception, from exception.hh
    SystemCall("", 0);
}

Optional<RasterHandle> CameraCocoa::get_next_frame()
{
    MutableRasterHandle raster_handle { width_, height_ };
    auto & raster = raster_handle.get();
    
    //basically gives buffer_info the buffer type, memory mmap, and index of 0
    
    switch (pixel_format_) {
        case V4L2_PIX_FMT_YUYV:
            <#statements#>
            break;
            
        default:
            break;
    }
}
