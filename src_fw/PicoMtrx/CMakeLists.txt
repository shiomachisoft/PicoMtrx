cmake_minimum_required(VERSION 3.13)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Initialise pico_sdk from installed location
# (note this can come from environment, CMake cache etc)

# == DO NOT EDIT THE FOLLOWING LINES for the Raspberry Pi Pico VS Code Extension to work ==
if(WIN32)
    set(USERHOME $ENV{USERPROFILE})
else()
    set(USERHOME $ENV{HOME})
endif()
set(sdkVersion 2.2.0)
set(toolchainVersion 14_2_Rel1)
set(picotoolVersion 2.2.0-a4)
set(picoVscode ${USERHOME}/.pico-sdk/cmake/pico-vscode.cmake)
if (EXISTS ${picoVscode})
    include(${picoVscode})
endif()
# ====================================================================================
set(PICO_BOARD pico CACHE STRING "Board type")

# Pull in SDK (must be before project)
include(pico_sdk_import.cmake)

project(PicoMtrx C CXX ASM)

# Initialize the SDK
pico_sdk_init()

add_executable(PicoMtrx)

target_sources(PicoMtrx	PRIVATE
		Matrix/Hal/hal_RGBMatrix_device.cpp
      	Matrix/ModuleDrivers/driver_RGBMatrix.cpp
      	Matrix/device/RGBMatrix_device.cpp
      	Matrix/Matrix.c
       	main.c
       	Timer.c
       	Common.c
      	Frame.c
      	Cmd.c
       	)

target_compile_options(PicoMtrx	PRIVATE
		-Wall
        -Wno-format          # int != int32_t as far as the compiler is concerned because gcc has int32_t as long int
        -Wno-unused-function # we have some for the docs that aren't called
        -Wno-maybe-uninitialized
        )

target_include_directories(PicoMtrx PRIVATE
    	${CMAKE_CURRENT_LIST_DIR}
    	${CMAKE_CURRENT_LIST_DIR}/Matrix
    	${CMAKE_CURRENT_LIST_DIR}/Matrix/Hal
    	${CMAKE_CURRENT_LIST_DIR}/Matrix/ModuleDrivers
    	${CMAKE_CURRENT_LIST_DIR}/Matrix/device
    	${CMAKE_CURRENT_LIST_DIR}/Matrix/config
    	)

# Configure TinyUSB to use a custom file
target_compile_definitions(PicoMtrx PRIVATE
		CFG_TUSB_CONFIG_FILE="my_tusb_config.h"
		)	

# pull in common dependencies
target_link_libraries(PicoMtrx PRIVATE
		pico_stdlib
		hardware_adc 
		hardware_spi 
		hardware_dma 
		hardware_i2c 
		pico_i2c_slave 
		hardware_pwm 
		pico_multicore
		hardware_sync
       	pico_unique_id
      	hardware_clocks
        hardware_flash
        hardware_exception
		)

# enable usb output, disable uart output
pico_enable_stdio_usb(PicoMtrx 1)
pico_enable_stdio_uart(PicoMtrx 0)

# create map/bin/hex/uf2 file etc.
pico_add_extra_outputs(PicoMtrx)