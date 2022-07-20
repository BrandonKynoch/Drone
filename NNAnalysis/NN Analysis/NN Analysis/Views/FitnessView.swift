//
//  FitnessView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/19.
//

import SwiftUI

struct FitnessView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    init() {
        DataHandler.singleton.GetMaxFitness()
    }
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .dark, opacity: 0.5)
            
            ZStack {
                FitnessAxisView(yMax: CGFloat(dataHandler.maxFitness))
                FitnessGraphView(yMax: CGFloat(dataHandler.maxFitness))
            }
            .padding()
        }
        .cornerRadius(CORNER_RADIUS)
        .padding()
    }
}

struct FitnessAxisView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    let yMax: CGFloat
    
    var body: some View {
        GeometryReader { geometry in
            let frameHeight = geometry.size.height
            let frameWidth = geometry.size.width
            
            let xSpacing = frameWidth / CGFloat(dataHandler.epochFolders.count)
            let ySpacing = frameHeight / CGFloat(dataHandler.UIFitnessYAxisArray.count)

            // X axis
            ForEach(dataHandler.UIEpochsArray, id: \.self) { i in
                let baseX = xSpacing * (CGFloat(i) + 1)
                
                // Line down the middle
                Path { path in
                    path.move(to: CGPoint(
                        x: baseX,
                        y: 0
                    ))
                    
                    path.addLine(to: CGPoint(x: baseX, y: frameHeight - 20))
                }
                .stroke(
                    COL_TEXT.opacity(0.1),
                    style: StrokeStyle(lineWidth: 1, lineCap: .round, lineJoin: .round)
                )
                
                Text("\(i + 1)")
                    .modifier(BodyTextModifier())
                    .foregroundColor(COL_TEXT)
                    .position(x: baseX, y: frameHeight)
            }
            
            // Y axis
            ForEach(dataHandler.UIFitnessYAxisArray, id: \.self) { i in
                let baseY = frameHeight - (ySpacing * (CGFloat(i) + 1))
    
                Path { path in
                    path.move(to: CGPoint(
                        x: 20,
                        y: baseY
                    ))
                    
                    path.addLine(to: CGPoint(x: frameWidth, y: baseY))
                }
                .stroke(
                    COL_TEXT.opacity(0.1),
                    style: StrokeStyle(lineWidth: 1, lineCap: .round, lineJoin: .round)
                )
                
                if i % 5 == 0 && i < dataHandler.UIFitnessYAxisArray.count - 1 {
                    Text(String(format: "%.1f", (Double(i) / Double(dataHandler.UIFitnessYAxisArray.count)) * Double(yMax)))
                        .modifier(BodyTextModifier())
                        .foregroundColor(COL_TEXT)
                        .position(x: 20, y: baseY)
                }
            }
        }
    }
}

struct FitnessGraphView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    let yMax: CGFloat
    
    var body: some View {
        GeometryReader { geometry in
            let frameHeight = geometry.size.height
            let frameWidth = geometry.size.width
            
            let xSpacing = frameWidth / CGFloat(dataHandler.epochFolders.count)
            let dotRadius: CGFloat = 6
            
            // Line
            Path { path in
                path.move(to: CGPoint(x: 20, y: frameHeight - 20))
                
                for i in 0..<dataHandler.epochFolders.count {
                    let baseX = xSpacing * (CGFloat(i) + 1)
                    let baseY = frameHeight - 20 - ((CGFloat(dataHandler.epochFolders[i].maxFitness) / yMax) * (frameHeight - 20))
    
                    path.addLine(to: CGPoint(x: baseX, y: baseY))
                }
            }
            .stroke(
                COL_ACC0,
                style: StrokeStyle(lineWidth: 3, lineCap: .round, lineJoin: .round)
            )
            
            // Main Dots
            Path { path in
                path.move(to: CGPoint(x: 20, y: frameHeight - 20 - dotRadius))
                
                path.addArc(center: CGPoint(x: 20, y: frameHeight - 20),
                            radius: dotRadius, startAngle: Angle(degrees: -90),
                            endAngle: .degrees(360.0), clockwise: false)

                for i in 0..<dataHandler.epochFolders.count {
                    let baseX = xSpacing * (CGFloat(i) + 1)
                    let baseY = frameHeight - 20 - ((CGFloat(dataHandler.epochFolders[i].maxFitness) / yMax) * (frameHeight - 20))
    
                    path.move(to: CGPoint(x: baseX, y: baseY - dotRadius))
                    
                    path.addArc(center: CGPoint(x: baseX, y: baseY),
                                radius: dotRadius, startAngle: Angle(degrees: -90),
                                endAngle: .degrees(360.0), clockwise: false)
                }
            }
            .stroke(COL_ACC2, lineWidth: 3)
            
            // Dots
            Path { path in
                for i in 0..<dataHandler.epochFolders.count {
                    for j in 0..<dataHandler.epochFolders[i].nns.count {
                        if dataHandler.epochFolders[i].maxFitness == dataHandler.epochFolders[i].nns[j].nng!.meta!.fitness! {
                            continue
                        }
                        
                        let baseX = xSpacing * (CGFloat(i) + 1)
                        let baseY = frameHeight - 20 - ((CGFloat(dataHandler.epochFolders[i].nns[j].nng!.meta!.fitness!) / yMax) * (frameHeight - 20))
        
                        path.move(to: CGPoint(x: baseX, y: baseY - dotRadius))
                        
                        path.addArc(center: CGPoint(x: baseX, y: baseY),
                                    radius: dotRadius, startAngle: Angle(degrees: -90),
                                    endAngle: .degrees(360.0), clockwise: false)
                    }
                }
            }
            .stroke(COL_ACC3, lineWidth: 3)
        }
    }
}

struct FitnessView_Previews: PreviewProvider {
    static var previews: some View {
        FitnessView()
    }
}
