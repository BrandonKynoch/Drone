//
//  NN.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/14/22.
//

import Foundation

class NN {
    public let name: String
    
    private(set) var neuralSize: Int = 0
    private(set) var neuralShape = [Int]()
    private(set) var activations = [Int]()
    private(set) var layers = [NNLayer]()
    
    private(set) var maxShape: Int = 0
    
    // UI VARS ///////////////////////////////////
    private(set) var UIIndexArray = [Int]() // Used for foreach loops in UI
    private(set) var UICountArray = [Int]() // Used for foreach loops in UI
    // UI VARS ///////////////////////////////////
    
    init(fromFile: URL) throws {
        name = fromFile.lastPathComponent
        
        let file = try FileHandle(forReadingFrom: fromFile)
        
        let fileData: Data = try file.readToEnd()!

        fileData.withUnsafeBytes({ pointer in
            var index = 0

            self.neuralSize = Int(pointer.ReadBytes(Int32.self, offset: &index))
            
            for i in 0..<neuralSize {
                let shape = Int(pointer.ReadBytes(Int32.self, offset: &index))
                if shape > maxShape {
                    maxShape = shape
                }
                neuralShape.append(shape)
                UICountArray.append(i)
            }
            
            for i in 0..<(neuralSize - 1) {
                activations.append(Int(pointer.ReadBytes(Int32.self, offset: &index)))
                UIIndexArray.append(i)
            }
            
            for i in 0..<(neuralSize - 1) {
                layers.append(NNLayer(
                    pointer: pointer,
                    shapeLeft: neuralShape[i],
                    shapeRight: neuralShape[i+1],
                    index: &index)
                )
            }
        })
    }
    
    // Represents the weights and biases used for transitioning from layer i to (i+1)
    struct NNLayer {
        var weights = [[Double]]()
        var biases = [Double]()
        
        // Index arrays just used for drawing UI
        private(set) var UIRowsArray = [Int]()
        private(set) var UIColsArray = [Int]()
        
        init(pointer: UnsafeRawBufferPointer, shapeLeft: Int, shapeRight: Int, index: inout Int) {
            // shapeLeft = number of columns
            // shapeRight = number of rows
            for _ in 0..<shapeLeft {
                weights.append([Double]())
            }
            
            // Using row major order
            for i in 0..<shapeLeft { // Columns
                for _ in 0..<shapeRight { // Rows
                    weights[i].append(pointer.ReadBytes(Double.self, offset: &index))
                }
                UIColsArray.append(i)
            }
            
            for i in 0..<shapeRight {
                biases.append(pointer.ReadBytes(Double.self, offset: &index))
                UIRowsArray.append(i)
            }
        }
    }
}
